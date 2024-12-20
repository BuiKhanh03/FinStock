using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Comment;
using api.Models;

namespace api.Mappers
{
    public static class CommentMappers
    {
        public static CommentDto ToCommentDto(this Comment commentModeDto)
        {
            return new CommentDto
            {
                Id = commentModeDto.Id,
                Title = commentModeDto.Title,
                Content = commentModeDto.Content,
                CreatedOn = commentModeDto.CreatedOn,
                StockId = commentModeDto.StockId
            };
        }
    }
}