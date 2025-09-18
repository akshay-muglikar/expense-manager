using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using AutoMapper;
using InventoryManagement.Api.Config;
using InventoryManagement.Api.Contracts;
using InventoryManagement.Api.Provider;
using InventoryManagement.Api.Utility;
using InventoryManagement.Domain.Model;
using InventoryManagement.Domain.Repository;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace InventoryManagement.Api.UseCase;

public class BillService
{
    private readonly IBillRepository _billRepository;
    private readonly IItemRepository _itemRepository;
    private readonly string _user;
    private readonly Guid _clientId;

    private readonly IMapper _mapper;
    private readonly InventoryConfig _inventoryConfig;
    public BillService(IBillRepository billRepository,
    IMapper mapper, IItemRepository itemRepository,
    UserServiceProvider userService, IOptions<InventoryConfig> options)
    {
        _billRepository = billRepository;
        _mapper = mapper;
        _itemRepository = itemRepository;
        _user = userService?.GetUsername();
        _clientId = Guid.Parse(userService?.GetClientId() ?? Guid.Empty.ToString());
        _inventoryConfig = options.Value;
    }


    public async Task<GetBillModel> GetByIdAsync(int id)
    {
        var bill = await _billRepository.GetByIdAsync(id);
        if (bill == null)
            throw new Exception("Bill not found");
        var billItems = await _billRepository.getBillItems(id);
        // Map Bill and BillItems to AddBillRequest
        var billItemModel = _mapper.Map<List<BillItemModel>>(billItems);
        Console.WriteLine($"BillItems {billItemModel.Count} {billItems.Count}");
        var billModel = _mapper.Map<GetBillModel>(bill);
        billModel.BillItems = billItemModel;
        return billModel;
    }

    public async Task<IEnumerable<GetAllBillModel>> GetAllAsync(DateTime? start, DateTime? end)
    {
        if (start == null && end == null)
        {
            var billItems = await _billRepository.GetAllAsync();
            return billItems.Select(x =>
            {
                var map = _mapper.Map<GetAllBillModel>(x.Item1);
                map.TotalAmount = x.Item2;
                return map;
            }).ToList();
        }
        else
        {
            var billItems = await _billRepository.GetAllAsync(start, end);
             return billItems.Select(x => {
                var map = _mapper.Map<GetAllBillModel>(x.Item1);
                map.TotalAmount = x.Item2;
                return map;
            }).ToList();
        }
    }

    public async Task<BillModel> AddAsync(BillModel billmodel)
    {
        Enum.TryParse(billmodel.Status, out BillStatus status);
        if (!Enum.TryParse(billmodel.PaymentMode, out PaymentMode mode))
        {
            mode = PaymentMode.CASH;
        }
        Bill bill = new Bill()
        {
            Name = billmodel.Name,
            Mobile = billmodel.Mobile,
            Discount = billmodel.Discount ?? 0,
            Advance = billmodel.Advance ?? 0,
            BillDate = billmodel.BillDate,
            Status = status,
            PaymentMode = mode,
            PaymentUser = _user,
            User = _user
        };

        await _billRepository.AddAsync(bill, _user);

        // Calculate the bill amount based on BillItems
        return _mapper.Map<BillModel>(bill);
    }

    public async Task UpdateAsync(BillModel billModel)
    {
        var bill = await _billRepository.GetByIdAsync(billModel.Id);
        if (bill == null)
            throw new Exception("Bill not found");

        Enum.TryParse(billModel.Status, out BillStatus status);
        if (!Enum.TryParse(billModel.PaymentMode, out PaymentMode mode))
        {
            mode = PaymentMode.CASH;
        }

        bill.Name = billModel.Name;
        bill.Mobile = billModel.Mobile;
        bill.Discount = billModel.Discount ?? 0;
        bill.Advance = billModel.Advance ?? 0;
        bill.BillDate = billModel.BillDate;
        bill.Status = status;
        bill.User = _user;
        bill.PaymentMode = mode;
        bill.PaymentUser = _user;

        await _billRepository.UpdateAsync(bill, _user);
    }

    public async Task DeleteAsync(int id, int billItemId)
    {
        var billItem = (await _billRepository.getBillItems(id)).FirstOrDefault(x => x.Id == billItemId);
        if (billItem != null)
        {
            var item = await _itemRepository.GetByIdAsync(billItem.ItemId);
            item.Quantity += billItem.Quantity;
            await _itemRepository.UpdateAsync(item, _user);
            await _billRepository.RemoveBillItem(billItem);
        }
    }
    public async Task Add(int id, BillItemModel billItemModel)
    {
        // // await _billRepository.AddBillItems(new List<BillItem>()
        // // {
        // //     new BillItem()
        // //     {
        // //         ItemId = billItemModel.ItemId,
        // //         Quantity = billItemModel.Quantity,
        // //         Amount = billItemModel.Amount,
        // //         BillId = id
        // //     }
        // // });
        // var item = await _itemRepository.GetByIdAsync(billItemModel.ItemId);


        // if (item == null)
        //     throw new Exception("Item not found");
        // item.Quantity -= billItemModel.Quantity;

        // await _itemRepository.Update(item);
    }

    public async Task DeleteAsync(int id)
    {
        await _billRepository.DeleteAsync(id, _user);
    }

    public async Task<Stream> GeneratePdf(int id)
    {
        var bill = await _billRepository.GetByIdAsync(id);
        if (bill == null)
            throw new Exception("Bill not found");

        var billitems = await _billRepository.getBillItems(bill.Id);
        // Generate PDF
        return InvoiceProvider.GetInvoice(bill, billitems);

    }

    public async Task SendMessage(int id)
    {
        var bill = await _billRepository.GetByIdAsync(id);
        if (bill == null)
            throw new Exception("Bill not found");
        var fileName = $"invoice_{id}.pdf";
        var tempFilePath = Path.Combine(Path.GetTempPath(), fileName);

        // 1. Generate and save PDF locally
        await using (var pdfStream = await GeneratePdf(id))
        await using (var fileStream = File.Create(tempFilePath))
        {
            await pdfStream.CopyToAsync(fileStream);
        }

        // 2. Upload PDF to WhatsApp
        var accessToken = _inventoryConfig.WhatsAppMessage.Token;
        var phoneNumberId = _inventoryConfig.WhatsAppMessage.PhoneId;
        var recipientPhone = $"91{bill.Mobile}";

        Console.WriteLine($" phoneid {phoneNumberId}\n {accessToken}\n\n {recipientPhone} \n\n");

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var form = new MultipartFormDataContent();
        await using var fileUploadStream = File.OpenRead(tempFilePath);
        var fileContent = new StreamContent(fileUploadStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(fileContent, "file", fileName);
        form.Add(new StringContent("whatsapp"), "messaging_product");
        form.Add(new StringContent("application/pdf"), "type");

        var uploadResponse = await client.PostAsync(
            $"https://graph.facebook.com/v22.0/{phoneNumberId}/media", form
        );

        var uploadJson = await uploadResponse.Content.ReadAsStringAsync();
        if (!uploadResponse.IsSuccessStatusCode)
        {
            Console.WriteLine("Upload failed:");
            Console.WriteLine(uploadJson);
            return;
        }
        Console.WriteLine(uploadJson);
        var mediaId = JsonDocument.Parse(uploadJson).RootElement.GetProperty("id").GetString();

        // 3. Send document message
        var payload = new
        {
            messaging_product = "whatsapp",
            to = bill.Mobile,
            type = "template",
            template = new
            {
                name = "receipt",
                language = new
                {
                    code = "en_US"
                },
                components = new object[]
                 {
                    new
                    {
                        type = "body",
                        parameters = new object[]
                        {
                            new { type = "text", text = bill.Mobile },
                            new { type = "text", text = bill.Id }
                        }
                    },
                    new
                    {
                        type = "button",
                        sub_type = "url",
                        index = 0,
                        parameters = new object[]
                        {
                           // new { type = "text", text = await GenerateDownloadLink(id) }
                        }
                    }
                 }
            }
        };


        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var messageResponse = await client.PostAsync(
            $"https://graph.facebook.com/v22.0/{phoneNumberId}/messages", content
        );

        var messageJson = await messageResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Message send response: {messageResponse.StatusCode}");
        Console.WriteLine(messageJson);
        Console.WriteLine($"PDF size: {new FileInfo(tempFilePath).Length} bytes");

        // 4. Cleanup
        File.Delete(tempFilePath);
    }

   

    public async Task<Stream> GeneratePdf(int id, Guid clientId, string apiKey)
    {
        // Validate the API key jwt token
        var tokenHandler = new JwtSecurityTokenHandler();
        tokenHandler.ValidateToken(apiKey, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_inventoryConfig.JwtSettings.UrlKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // Remove delay of token when expire
        }, out SecurityToken validatedToken);

        var token = tokenHandler.ReadJwtToken(apiKey);
        var clientIdClaim = token.Claims.FirstOrDefault(c => c.Type == "client_id");
        // check token expiry
        var expiryClaim = token.Claims.FirstOrDefault(c => c.Type == "exp");
        if (expiryClaim != null)
        {
            var expiryDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiryClaim.Value));
            if (expiryDate < DateTimeOffset.UtcNow)
            {
                throw new Exception("Token expired");
            }
        }
        if (clientIdClaim == null || clientIdClaim.Value != clientId.ToString())
        {
            throw new Exception("Invalid client id");
        }



        return await GeneratePdf(id);
    }

    public async Task<BillModel> AddBillWithItemsAsync(AddBillRequest request)
    {
        Enum.TryParse(request.Status, out BillStatus status);
        if (!Enum.TryParse(request.PaymentMode, out PaymentMode mode))
        {
            mode = PaymentMode.CASH;
        }
        Bill bill = new Bill()
        {
            Name = request.Name,
            Mobile = request.Mobile,
            Discount = request.Discount ?? 0,
            Advance = request.Advance ?? 0,
            BillDate = request.BillDate,
            Status = status,
            PaymentMode = mode,
            PaymentUser = _user,
            User = _user
        };
        var billItems = request.BillItems?.ConvertAll(bi => new BillItem
        {
            ItemId = bi.ItemId,
            Quantity = bi.Quantity,
            Amount = bi.Amount
        }) ?? new List<BillItem>();
        await _billRepository.AddBillWithItemsAsync(bill, billItems, _user);
        return _mapper.Map<BillModel>(bill);
    }
    
    public async Task<BillModel> UpdateBillWithItemsAsync(int id, AddBillRequest request)
    {
        Enum.TryParse(request.Status, out BillStatus status);
        if (!Enum.TryParse(request.PaymentMode, out PaymentMode mode))
        {
            mode = PaymentMode.CASH;
        }
        var existingBill = await _billRepository.GetByIdAsync(id);
        if (existingBill == null)
            throw new Exception("Bill not found");
        existingBill.Name = request.Name;
        existingBill.Mobile = request.Mobile;
        existingBill.Discount = request.Discount ?? 0;
        existingBill.Advance = request.Advance ?? 0;
        existingBill.BillDate = request.BillDate;
        existingBill.Status = status;
        existingBill.PaymentMode = mode;
        existingBill.PaymentUser = _user;
        existingBill.User = _user;
        await _billRepository.UpdateAsync(existingBill, _user);
        
        await _billRepository.RemoveBillItems(id);

        var billItems = request.BillItems?.ConvertAll(bi => new BillItem
        {
            BillId = id,
            ItemId = bi.ItemId,
            Quantity = bi.Quantity,
            Amount = bi.Amount
        }) ?? new List<BillItem>();
        await _billRepository.AddBillItems(id, billItems, _user);
        return _mapper.Map<BillModel>(existingBill);
    }

    internal List<Dictionary<string, object>> ExecuteSqlQuery(string sqlQuery)
    {
        return _billRepository.ExecuteSqlQuery(sqlQuery);
    }
}

