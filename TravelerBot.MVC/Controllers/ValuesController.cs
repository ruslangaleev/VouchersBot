using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using TravelerBot.Api.ResourceModels;
using TravelerBot.MVC.Data.Models;
using TravelerBot.MVC.Data.Repositories;
using TravelerBot.MVC.Data.Repositories.Logic;
using TravelerBot.MVC.ResourceModels;
using Action = TravelerBot.Api.ResourceModels.Action;

namespace TravelerBot.MVC.Controllers
{
    public class ValuesController : ApiController
    {
        private readonly string _secret = "q11w22e33r44";

        private readonly string _token = "195b278ff87d740cd559f757783973b80a545b828dd9ca102f9e100c5593290afb30cc38e00e6a714f06a";

        private readonly static HttpClient _httpClient = new HttpClient();

        // POST api/values
        public async Task<object> Post([FromBody]Message message)
        {
            if (message.Type == "confirmation")
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent("6bc74ded", Encoding.UTF8, "text/plain");
                return response;
            }

            if (message.Secret != _secret)
            {
                return BadRequest("Не верный секретный ключ.");
            }

            if (message.Type == "message_new")
            {
                var userRepo = new UserRepository();

                var user = await userRepo.Get(message.ObjectMessage.UserId);

                var responseModel = new ResponseModel();

                if (user != null)
                {
                    if (user.Role.Name == "ADMIN")
                    {
                        responseModel = await NewMethod(message, userRepo, user);
                    }

                    if (user.Role.Name == "PUBLISHER")
                    {
                        responseModel = NewMethod1(message, userRepo, user);
                    }
                }

                var request = "https://api.vk.com/method/messages.send?user_id={message.ObjectMessage.UserId}&group_id={message.GroupId}&message={responseModel.Message}&v=5.80&access_token={_token}";

                await _httpClient.GetAsync(request);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent("ok", Encoding.UTF8, "text/plain");
                return response;
            }

            return BadRequest("тип события не распознан.");

            // "https://api.vk.com/method/messages.send?user_id={message.ObjectMessage.UserId}&group_id={message.GroupId}&message={responseModel.Message}&v=5.80&access_token={_token}"
        }

        private ResponseModel NewMethod1(Message message, UserRepository userRepo, User user)
        {
            if (message.ObjectMessage.Body == "Активировать")
            {
                if (user.TransactionType != TransactionType.NoActivate)
                {
                    return new ResponseModel
                    {
                        Message = "Ваш аккаунт уже активирован."
                    };
                }

                user.TransactionType = TransactionType.Activate;
                userRepo.Update(user);
                userRepo.SaveChanges();

                return new ResponseModel
                {
                    Message = "Введите код для активации аккаунта."
                };
            }

            if (user.TransactionType == TransactionType.Activate)
            {
                var oneTimePassword = JsonConvert.DeserializeObject<OneTimePassword>(user.AdditionalInfo);
                if (oneTimePassword.LifeTime < DateTime.UtcNow.AddHours(5))
                {
                    return new ResponseModel
                    {
                        Message = "Код активации просрочен"
                    };
                }

                user.IsBlocked = false;
                userRepo.Update(user);
                userRepo.SaveChanges();
            }

            return new ResponseModel
            {
                Message = "Команда не распознана"
            };
        }

        private async Task<ResponseModel> NewMethod(Message message, UserRepository userRepo, User user)
        {
            if (user.TransactionType == TransactionType.Start)
            {
                var buttons = new Button[]
                {
                    new Button
                    {
                        color = "positive",
                        action = new Action
                        {
                            label = "Добавить публикатора",
                            type = "text",
                            payload = JsonConvert.SerializeObject(new
                            {
                                button = "1"
                            })
                        }
                    },
                };

                return new ResponseModel
                {
                    Message = $"Приветствую тебя в режиме администратора.",
                    Keyboard = new Keyboard
                    {
                        OneTime = false,
                        buttons = new[]
                        {
                            buttons
                        }
                    }
                };
            }

            if (message.ObjectMessage.Body == "Добавить публикатора")
            {
                user.TransactionType = TransactionType.AddPublisher;
                userRepo.Update(user);
                userRepo.SaveChanges();

                return new ResponseModel
                {
                    Message = "Необходимо указать идентификатор аккаунта пользователя в Вконтакте (только цифры)"
                };
            }

            if (user.TransactionType == TransactionType.AddPublisher)
            {
                var roleUser = await userRepo.GetRoleAsync("PUBLISHER");

                var userFound = await userRepo.Get(Convert.ToInt32(message.ObjectMessage.Body));
                if (userFound != null)
                {
                    return new ResponseModel
                    {
                        Message = "Пользователь с таким идентификатором аккаунта уже зарегистрирован."
                    };
                }

                var random = new Random();
                var password = random.Next(99999);

                userRepo.Add(new User
                {
                    AccountId = Convert.ToInt32(message.ObjectMessage.Body),
                    CreateAt = DateTime.Now.AddHours(5),
                    UserId = Guid.NewGuid(),
                    Role = roleUser,
                    AdditionalInfo = JsonConvert.SerializeObject(new OneTimePassword
                    {
                        LifeTime = DateTime.UtcNow.AddHours(6),
                        Password = password.ToString()
                    }),
                    TransactionType = TransactionType.Start,
                    StateType = StateType.NotActivated,
                });
                //userRepo.SaveChanges();

                user.TransactionType = TransactionType.Start;
                userRepo.Update(user);
                userRepo.SaveChanges();

                var buttons = new Button[]
                {
                    new Button
                    {
                        color = "positive",
                        action = new Action
                        {
                            label = "Добавить публикатора",
                            type = "text",
                            payload = JsonConvert.SerializeObject(new
                            {
                                button = "1"
                            })
                        }
                    },
                };

                return new ResponseModel
                {
                    Message = $"Пользователь успешно зарегистрирован. Необходимо передать указанному пользователю код для активации аккаунта: {password}",
                    Keyboard = new Keyboard
                    {
                        OneTime = false,
                        buttons = new[]
                        {
                            buttons
                        }
                    }
                };
            }

            return new ResponseModel
            {
                Message = "Команда не распознана"
            };
        }
    }
}
