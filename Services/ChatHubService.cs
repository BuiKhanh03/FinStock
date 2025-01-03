using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Chat;
using api.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using StackExchange.Redis;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace api.Services
{
    // [Authorize]
    public class ChatHub : Hub
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IConfiguration _config;
        private static Dictionary<string, string> _userConnections = new Dictionary<string, string>();
        public ChatHub(IConnectionMultiplexer redis, IConfiguration config)
        {
            _redis = redis;
            _config = config;
        }
        public override Task OnConnectedAsync()
        {
            var token = Context.GetHttpContext()?.Request?.Headers["Authorization"].ToString();
            Console.WriteLine(Context.User.Identity.IsAuthenticated);
            var userId = Context.User.Claims
               .FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
            if (!_userConnections.ContainsKey(userId))
            {
                _userConnections.Add(userId, Context.ConnectionId);
            }

            return base.OnConnectedAsync();
        }

        public async Task NewMessage(string senderId, string receiverId, string message)
        {
            // Tạo chatRoomKey cho cuộc trò chuyện
            string chatRoomKey = GetChatRoomKey(senderId, receiverId);

            // Lấy database Redis
            var db = _redis.GetDatabase();

            // Kiểm tra loại dữ liệu của key
            var type = await db.KeyTypeAsync(chatRoomKey);

            // Nếu key không phải là List, xóa key cũ để tránh lỗi
            if (type != RedisType.List)
            {
                // Xóa key nếu kiểu không phải là List
                await db.KeyDeleteAsync(chatRoomKey);
            }

            // Tạo đối tượng tin nhắn
            var chatMessage = new ChatDto
            {
                UserId = senderId,
                Message = message,
                Timestamp = DateTime.Now
            };

            if (_userConnections.ContainsKey(senderId) && _userConnections.ContainsKey(receiverId))
            {
                // Lấy Connection ID của người gửi và người nhận
                var senderConnectionId = _userConnections[senderId];
                var receiverConnectionId = _userConnections[receiverId];

                // Gửi tin nhắn tới người gửi và người nhận
                if (senderConnectionId != receiverConnectionId)
                {
                    await Clients.Client(senderConnectionId).SendAsync("receiveMessage", message);
                }
                await Clients.Client(receiverConnectionId).SendAsync("receiveMessage", message);
            }

            // Chuyển đối tượng thành chuỗi JSON
            string jsonMessage = JsonConvert.SerializeObject(chatMessage);

            // Lưu tin nhắn vào Redis (sử dụng List để lưu các tin nhắn)
            await db.ListRightPushAsync(chatRoomKey, jsonMessage);

            // // Gửi tin nhắn tới tất cả client của hai người
            await Clients.User(senderId.ToString()).SendAsync("messageReceived", chatMessage);
            await Clients.User(receiverId.ToString()).SendAsync("messageReceived", chatMessage);
        }

        // Lấy lịch sử tin nhắn giữa hai người
        public async Task GetChatHistory(string user1Id, string user2Id)
        {
            // Tạo chatRoomKey cho cuộc trò chuyện
            string chatRoomKey = GetChatRoomKey(user1Id, user2Id);

            var db = _redis.GetDatabase();

            // Kiểm tra xem phòng trò chuyện đã tồn tại chưa
            bool chatRoomExists = await db.KeyExistsAsync(chatRoomKey);
            if (!chatRoomExists)
            {
                await Clients.Caller.SendAsync("messageReceived", "No messages found.");
                return;
            }
            // Lấy các tin nhắn lịch sử (max 50 tin nhắn gần nhất)
            var chatHistory = await db.ListRangeAsync(chatRoomKey, 0, 50);
            foreach (var message in chatHistory)
            {
                // Phân tích chuỗi JSON thành đối tượng ChatMessage
                var chatMessage = JsonConvert.DeserializeObject<ChatDto>(message.ToString());

                // Gửi lại tin nhắn cho client, có thể bao gồm cả người gửi
                string formattedMessage = $"{(chatMessage.UserId == user1Id ? "User 1" : "User 2")}: {chatMessage.Message}";
                await Clients.Caller.SendAsync("messageReceived", formattedMessage);
            }
        }

        // Khi người dùng kết nối, lưu Connection ID của họ

        // Khi người dùng rời khỏi, xóa Connection ID của họ
        public override Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;  // Lấy UserId
            Console.WriteLine("userId: " + userId);
            if (_userConnections.ContainsKey(userId))
            {
                _userConnections.Remove(userId);
            }

            return base.OnDisconnectedAsync(exception);
        }

        // Hàm tạo key cho cuộc trò chuyện giữa hai người
        private string GetChatRoomKey(string user1Id, string user2Id)
        {
            return $"{(user1Id.CompareTo(user2Id) < 0 ? user1Id : user2Id)}:{(user1Id.CompareTo(user2Id) > 0 ? user1Id : user2Id)}";
        }

        private ClaimsPrincipal ValidateToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadToken(token.Replace("Bearer ", "")) as JwtSecurityToken;
                if (jwtToken == null) return null;

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:SigningKey"]));
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config["JWT:Issuer"],
                    ValidAudience = _config["JWT:Audience"],
                    IssuerSigningKey = key
                };

                return handler.ValidateToken(token.Replace("Bearer ", ""), validationParameters, out _);
            }
            catch
            {
                return null;
            }
        }
    }
}