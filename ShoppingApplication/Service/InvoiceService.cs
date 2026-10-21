using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ShoppingApplication.Model;
using ShoppingApplication.Repository;
using System.Diagnostics;

namespace ShoppingApplication.Service
{
    internal class InvoiceService
    {
        private readonly InvoiceRepository _repo;
        public InvoiceService(InvoiceRepository repo)
        {
            _repo = repo;
        }

        public Invoice Display(User user, CartService cart) => new()
        {
            Number = $"INV-{DateTime.Now:yyyyMMdd-HHmmss}-{user.Username}",
            Date = DateTime.Now,
            Customer = user,
            Items = cart.Items.ToList(),
            AppliedCouponCode = cart.AppliedCoupon?.Code ?? string.Empty,
            DiscountAmount = cart.DiscountTotal
        };

        public string CreatePdf(Invoice inv)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var path = _repo.PathFor(inv.Number);
            Document.Create(doc => doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(h =>
                {
                    h.Item().Text("INVOICE").FontSize(22).Bold();
                    h.Item().Text($"Invoice No: {inv.Number}");
                    h.Item().Text($"Date: {inv.Date:dd/MM/yyyy} And Time: {inv.Date:HH:mm:ss}");
                });

                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Spacing(6);
                    col.Item().Text($"Username   : {inv.Customer.Username}");
                    col.Item().Text($"First Name : {inv.Customer.FirstName}");
                    col.Item().Text($"Last Name  : {(string.IsNullOrWhiteSpace(inv.Customer.LastName) ? "-" : inv.Customer.LastName)}");
                    col.Item().Text($"Deliver To : {inv.DeliveryAddress}");
                    col.Item().PaddingTop(10);

                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(55);
                            c.RelativeColumn();
                            c.ConstantColumn(40);
                            c.ConstantColumn(80);
                            c.ConstantColumn(90);
                        });

                        t.Header(h =>
                        {
                            h.Cell().Element(CellStyle).Text("ID").Bold();
                            h.Cell().Element(CellStyle).Text("Product").Bold();
                            h.Cell().Element(CellStyle).AlignRight().Text("Qty").Bold();
                            h.Cell().Element(CellStyle).AlignRight().Text("Price").Bold();
                            h.Cell().Element(CellStyle).AlignRight().Text("Total").Bold();
                        });

                        foreach (var i in inv.Items)
                        {
                            t.Cell().Element(CellStyle).Text(i.Product.Id);
                            t.Cell().Element(CellStyle).Text(i.Product.Name);
                            t.Cell().Element(CellStyle).AlignRight().Text(i.Quantity.ToString());
                            t.Cell().Element(CellStyle).AlignRight().Text($"Rs {i.Product.Price:N2}");
                            t.Cell().Element(CellStyle).AlignRight().Text($"Rs {i.Total:N2}");
                        }
                    });

                    col.Item().PaddingTop(15).AlignRight().Column(totalCol =>
                    {
                        totalCol.Spacing(2);
                        totalCol.Item().Text($"Subtotal: Rs {inv.Total:N2}").FontSize(11);
                        if (!string.IsNullOrWhiteSpace(inv.AppliedCouponCode))
                        {
                            totalCol.Item().Text(t =>
                            {
                                t.Span($"Discount ({inv.AppliedCouponCode}): -Rs {inv.DiscountAmount:N2}")
                                 .Style(TextStyle.Default.FontSize(11));
                            });
                        }

                        totalCol.Item().Text($"Grand Total: Rs {inv.GrandTotal:N2}").Bold().FontSize(13);

                        totalCol.Item().Text(t =>
                        {
                            t.Span("Payment Status: PAID")
                             .Style(TextStyle.Default.Bold());
                        });
                    });
                });

                page.Footer().AlignCenter().Text("Thank you for shopping with us!");
            })).GeneratePdf(path);

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PDF created successfully at: {path}. Could not auto-launch: {ex.Message}");
            }

            return path;
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingVertical(4);
        }
    }
}
