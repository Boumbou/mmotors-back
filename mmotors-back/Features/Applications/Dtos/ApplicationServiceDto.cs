/* 
    * this file defines Dtos for applications
    * these Dtos are used to transfer data between the client and the server
*/

using mmotors_back.Models;
namespace mmotors_back.Features.Applications.Dtos
{
    public class ApplicationServiceDto
    {
        public int ServiceId { get; set; }
        public OverheadType AppliedOverheadType { get; set; }
        public decimal AppliedOverheadValue { get; set; }
        public decimal CalculatedOverheadAmount { get; set; }
    }
}