using AutoMapper;
using Buckeye.Lending.Api.Dtos;
using Buckeye.Lending.Api.Models;

namespace Buckeye.Lending.Api.Mappings;

public class ReviewQueueProfile : Profile
{
    public ReviewQueueProfile()
    {
        // Flatten LoanApplication
        CreateMap<ReviewItem, ReviewItemResponse>()
            .ForMember(d => d.ApplicantName, o => o.MapFrom(s => s.LoanApplication.ApplicantName))
            .ForMember(d => d.Amount, o => o.MapFrom(s => s.LoanApplication.LoanAmount))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.LoanApplication.Status));

        // Compute values
        CreateMap<ReviewQueue, ReviewQueueResponse>()
            .AfterMap((src, dest) =>
            {
                dest.TotalItems = dest.Items.Count;
                dest.HighPriorityCount = dest.Items.Count(i => i.Priority == 1);
                dest.MediumPriorityCount = dest.Items.Count(i => i.Priority is 2 or 3);
                dest.LowPriorityCount = dest.Items.Count(i => i.Priority >= 4);
            });
    }
}
