using AutoMapper;
using ChatOnlineV2.Data.Entities;
using ChatOnlineV2.Helpers;
using ChatOnlineV2.Models;

namespace ChatOnlineV2.Mappings
{
    public class MessageProfile : Profile
    {
        public MessageProfile()
        { // tạo 1 create map từ from
            CreateMap<Message, MessageViewModel>()
                .ForMember(dst => dst.From, opt => opt.MapFrom(x => x.FromUser.FullName))
                .ForMember(dst => dst.Room, opt => opt.MapFrom(x => x.ToRoom.Name))
                .ForMember(dst => dst.Avatar, opt => opt.MapFrom(x => x.FromUser.Avatar))
                .ForMember(dst => dst.Content, opt => opt.MapFrom(x => BasicEmojis.ParseEmojis(x.Content)))
                .ForMember(dst => dst.Timestamp, opt => opt.MapFrom(x => x.Timestamp));
            CreateMap<MessageViewModel, Message>();
        }
    }
}
