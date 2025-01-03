using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using api.Dtos.Chat;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace api.Controllers
{
    [Route("api/chat")]
    [ApiController]

    public class ChatController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IConfiguration _config;
        private readonly UserManager<AppUser> _userManager;

        public ChatController(IConnectionMultiplexer redis, UserManager<AppUser> userManager, IConfiguration config)
        {
            _redis = redis;
            _userManager = userManager;
            _config = config;
        }

        [HttpGet("chat-history/{userId}/{id}")]
        [Authorize]
        public async Task<IActionResult> GetChatHistory(string userId, string id)
        {

            string chatRoomKey = GetChatRoomKey(userId, id);
            var db = _redis.GetDatabase();

            bool chatRoomExists = await db.KeyExistsAsync(chatRoomKey);
            if (!chatRoomExists)
            {
                return Ok(new { message = "No messages found." });
            }

            var chatHistory = await db.ListRangeAsync(chatRoomKey, 0, 50);
            var messages = chatHistory.Select(message => JsonConvert.DeserializeObject<ChatDto>(message.ToString())).ToList();

            return Ok(messages);
        }

        private string GetChatRoomKey(string user1Id, string user2Id)
        {
            return $"{(user1Id.CompareTo(user2Id) < 0 ? user1Id : user2Id)}:{(user1Id.CompareTo(user2Id) > 0 ? user1Id : user2Id)}";
        }

        private ClaimsPrincipal? GetTokenPrincipal(string token)
        {

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("Jwt:SigningKey").Value));

            var validation = new TokenValidationParameters
            {
                IssuerSigningKey = securityKey,
                ValidateLifetime = false,
                ValidateActor = false,
                ValidateIssuer = true,
                ValidIssuer = _config["JWT:Issuer"],
                ValidateAudience = true,
                ValidAudience = _config["JWT:Audience"],
            };
            return new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
        }
    }
}