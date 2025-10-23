using System.ComponentModel.DataAnnotations;

namespace LopTopWebApi.Contracts
{
    public sealed class AddRatingRequest
    {
        [Range(1, 5)]
        public int Rating { get; set; } // 1 to 5          

        public string? Comment { get; set; } //optional     
    }
}
