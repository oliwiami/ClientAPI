using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ClientAPI.Models;
using ClientAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace ClientAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ClientController : Controller
    {
        private readonly ApiContext dbContext;
        public ClientController(ApiContext dbContext)
        {   
            this.dbContext = dbContext;
        }

        //Create client
        [HttpPost]
        public async Task<IActionResult> AddClient(AddRequest addRequest)
        {
            var client = new ClientModel()
            {
                Id = Guid.NewGuid(),
                Pesel = addRequest.Pesel,
                Name = addRequest.Name,
                LastName = addRequest.LastName
            };

            await dbContext.Clients.AddAsync(client);

            await dbContext.SaveChangesAsync();
            return Ok(client);
        }

        //Get info from pesel
        [HttpGet]
        [Route("{pesel}")]
        public async Task<IActionResult> GetAge([FromRoute] string pesel)
        {
            var client = dbContext.Clients.FirstOrDefault(c => c.Pesel == pesel);
            if (client == null)
            {
                return NotFound();
            }

            int age;
            int decadeIndycator =(int)Char.GetNumericValue(pesel[2]);
            String birthString;
            DateTime dateOfBirth=DateTime.Now;
            DateTime today = DateTime.Now;

            String discount, wishes;

            List<String>result=new List<String>();

            //Get age
            if (decadeIndycator >= 2)
            {
                string month = (decadeIndycator - 2).ToString();
                birthString = "20" + pesel[0] + pesel[1] + "-" + month + pesel[3] + "-" + pesel[4] + pesel[5];

                dateOfBirth = Convert.ToDateTime(birthString);

                TimeSpan ts = today.Subtract(dateOfBirth);
                age = ts.Days/365;
               
            }
            else
            {

                birthString = "19" + pesel[0] + pesel[1] + "-" + pesel[2] + pesel[3] + "-" + pesel[4] + pesel[5];
                dateOfBirth = Convert.ToDateTime(birthString);

                TimeSpan ts = today.Subtract(dateOfBirth);
                age = ts.Days / 365;
            }

            result.Add(age.ToString());

            //Get discount 
            if (today.Day == dateOfBirth.Day && today.Month==dateOfBirth.Month)
            {
                discount = "Promocja wynosi 10%";
            }
            else if (today.Month == dateOfBirth.Month)
            {
                discount = "Promocja wynosi 5%";
            }
            else if (today.Month >= 02 && today.Month <= 09)
            {
                discount = "Promocja wynosi 0%";
            }
            else
            {
                discount = "Promocja wynosi 2.5%";
            }

            result.Add(discount.ToString());

            //Get Birthday wishes
            if (today.Day == dateOfBirth.Day && today.Month == dateOfBirth.Month)
            {
                int g = (int)Char.GetNumericValue(pesel[pesel.Length - 2]);
                int genderIndicator = g % 2;

                switch (genderIndicator)
                {
                    case 0:
                        wishes = "Klientko " + client.Name + " " + client.LastName + "! Życzymy Ci sto lat!";
                        break;
                    case 1:
                        wishes = "Kliencie " + client.Name + client.LastName + "! Życzymy Ci sto lat!";
                        break;
                    default:
                        wishes = "";
                        break;
                }
                result.Add(wishes.ToString());


            }

            return Ok(result);
        }
    }
}
