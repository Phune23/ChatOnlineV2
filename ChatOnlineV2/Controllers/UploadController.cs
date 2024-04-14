using AutoMapper;
using ChatOnlineV2.Data;
using ChatOnlineV2.Data.Entities;
using ChatOnlineV2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace ChatOnlineV2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly int FileSizeLimit; //giới hạn kích thước file
        private readonly string[] AllowedExtensions; //lọc đuôi file 
        private readonly ManageChatDbContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment; // lấy đc folder wwwrot để vào các file trong đó 

        public UploadController(ManageChatDbContext context,
            IMapper mapper,
            IWebHostEnvironment environment,

            IConfiguration configruation)
            {
            _context = context;
            _mapper = mapper;
            _environment = environment;


            FileSizeLimit = configruation.GetSection("FileUpload").GetValue<int>("FileSizeLimit"); //gới hạn kích thước file trỏ tới FileUpload và sau đó tới phần FileSizeLimit
            AllowedExtensions = configruation.GetSection("FileUpload").GetValue<string>("AllowedExtensions").Split(","); // xóa bỏ các giấu phẩy trong appestting.Development.json => "AllowedExtensions": ".jpg,.jpeg,.png"
            }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload([FromForm] UploadViewModel uploadViewModel)
        {
            if (ModelState.IsValid)
            {
                if (!Validate(uploadViewModel.File))
                {
                    return BadRequest("Validation failed!");
                }
                //lấy thoe file namr
                var fileName = DateTime.Now.ToString("yyyymmddMMss") + "_" + Path.GetFileName(uploadViewModel.File.FileName);
                var folderPath = Path.Combine(_environment.WebRootPath, "uploads"); //dẫn đến file uploads trong wwwroot
                var filePath = Path.Combine(folderPath, fileName); //gộp fileName và folderPath
                if (!Directory.Exists(folderPath)) //nếu ko có file uploads thì tự tọa ra file 
                    Directory.CreateDirectory(folderPath);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadViewModel.File.CopyToAsync(fileStream); //copy file vài folder
                }
                //bắt đầu upload
                var user = _context.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault(); //lấy user đang đăng nhập
                var room = _context.Rooms.Where(r => r.Id == uploadViewModel.RoomId).FirstOrDefault(); //lấy room của tài khoản đó
                if (user == null || room == null) //nếu không tìm thấy 1 trong 2 cái ở trên thì trả về not found
                    return NotFound();
                //lấy ảnh
                string htmlImage = string.Format(
                    "<a href=\"/uploads/{0}\" target=\"_blank\">" +
                    "<img src=\"/uploads/{0}\" class=\"post-image\">" +
                    "</a>", fileName);
                // tạo tra 1 message
                var message = new Message()
                {
                    //trong content phần Replace kiểm tra xem có đúng định dang ko, nếu không thì trả về null
                    Content = Regex.Replace(htmlImage, @"(?i)<(?!img|a|/a|/img).*?>", string.Empty),
                    Timestamp = DateTime.Now,
                    FromUser = user,
                    ToRoom = room
                };
                //add message vào database
                await _context.Messages.AddAsync(message);
                await _context.SaveChangesAsync();

                // tạo messageViewModel cho những ng trong cùng room thấy
                var messageViewModel = _mapper.Map<Message, MessageViewModel>(message);
                //   await _hubContext.Clients.Group(room.Name).SendAsync("newMessage", messageViewModel);

                return Ok();
            }

            return BadRequest();
        }

        private bool Validate(IFormFile file) //sử lý uploand ảnh
        {
            if (file.Length > FileSizeLimit) //nếu file lớn hơn giới hạn thì ta ko uploand
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant(); 
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Any(s => s.Contains(extension))) //sau khi kiểm tra có null ko sau đó cho phép các đuôi file đã đc ghi trong AllowedExtensions
                return false;

            return true;
        }
    }
}
