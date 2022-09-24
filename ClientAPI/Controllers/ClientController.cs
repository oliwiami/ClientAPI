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
            try
            {
                var client = new ClientModel()
                {
                    Id = Guid.NewGuid(),
                    Pesel = addRequest.Pesel,
                    Name = addRequest.Name,
                    LastName = addRequest.LastName
                };

                bool validatePesel(String pesel)
                {
                    int[] weights = { 1, 3, 7, 9, 1, 3, 7, 9, 1, 3 };

                    bool result = false;

                    if (pesel.Length == 11)
                    {
                        int controlSum = 0;
                        for (int i = 0; i < pesel.Length - 1; i++)
                        {
                            controlSum += weights[i] * int.Parse(pesel[i].ToString());
                        }
                        int controlNum = controlSum % 10;
                        controlNum = 10 - controlNum;
                        if (controlNum == 10)
                        {
                            controlNum = 0;
                        }
                        int lastNum = int.Parse(pesel[pesel.Length - 1].ToString());

                        result = controlNum == lastNum;
                    }
                    return result;
                }

                bool isPeselValid = validatePesel(client.Pesel);

                if (isPeselValid)
                {
                    await dbContext.Clients.AddAsync(client);

                    await dbContext.SaveChangesAsync();
                    return Ok(client);
                }
                else
                {
                    return BadRequest("Invalid pesel input");
                }
            }catch (Exception ex)
            {
                return BadRequest("Could not create client");
            }

        }

        //Get info from pesel
        [HttpGet]
        [Route("{pesel}")]
        public async Task<IActionResult> GetClient([FromRoute] string pesel)
        {
            var client = dbContext.Clients.FirstOrDefault(c => c.Pesel == pesel);
            if (client == null)
            {
                return NotFound("Client with pesel: " + pesel +" does not exist");
            }

            int age;
            int milleniumIndicator =(int)Char.GetNumericValue(pesel[2]);
            String birthString;
            DateTime dateOfBirth=DateTime.Now;
            DateTime today = DateTime.Now;

            String discount, wishes;

            List<String>result=new List<String>();

            //Get age
            if (milleniumIndicator >= 2)
            {
                string month = (milleniumIndicator - 2).ToString();
                birthString = "20" + pesel[0] + pesel[1] + "-" + month + pesel[3] + "-" + pesel[4] + pesel[5];

                dateOfBirth = Convert.ToDateTime(birthString);

                age = today.Year - dateOfBirth.Year;
                if(today.Month < dateOfBirth.Month)
                {
                    age--;
                }else if(today.Month == dateOfBirth.Month && today.Day < dateOfBirth.Day)
                {
                    age--;
                }
               
            }
            else
            {

                birthString = "19" + pesel[0] + pesel[1] + "-" + pesel[2] + pesel[3] + "-" + pesel[4] + pesel[5];
                dateOfBirth = Convert.ToDateTime(birthString);

                age = today.Year - dateOfBirth.Year;
                if (today.Month < dateOfBirth.Month)
                {
                    age--;
                }
                else if (today.Month == dateOfBirth.Month && today.Day < dateOfBirth.Day)
                {
                    age--;
                }
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
                        wishes = "Kliencie " + client.Name + " " + client.LastName + "! Życzymy Ci sto lat!";
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
