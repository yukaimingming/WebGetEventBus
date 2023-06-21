using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using WebGetEventBus.common;
using WebGetEventBus.models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebGetEventBus.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class EventBusController : ControllerBase
    {
        #region Fields

        private readonly ILogger<EventBusController> _logger;

        private readonly IConfiguration _Configuration;

        private readonly IGetxml _getxml;

        #endregion Fields

        #region Constructors

        public EventBusController(ILogger<EventBusController> logger, IConfiguration configuration, IGetxml param)
        {
            _logger = logger;
            _Configuration = configuration;
            _getxml = param;
        }

        #endregion Constructors

        // GET: api/<EventBusController>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET api/<EventBusController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        #region Methods

        // POST api/<EventBusController>
        /// <summary>
        ///   接收eventbusdata数据
        /// </summary>
        /// <param name="value">
        /// </param>
        [HttpPost("EventBustResult")]
        public async Task<IActionResult> Post([FromBody] object value)
        {
            _logger.LogInformation(value.ToString());
            try
            {
                if (value == null)
                {
                    return BadRequest();
                }

                var newvalue = value?.ToString()?.Replace("\"{", "{").Replace("}\"", "}").Replace("\\", "");

                Root? obj = JsonConvert.DeserializeObject<Root>(newvalue);

                var depot_code = obj?.data?.main?.set_depot_id;

                if (depot_code != _Configuration["header:depot_id"])
                {
                    return BadRequest(new { code = 400, Message = "暂无店铺ID" });
                }

                //生成webservice入参soap xml
                string soapxml = _getxml.GetXml(obj);

                //调用soap的webservice
                _logger.LogInformation(soapxml);

                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(_Configuration["header:weburl"].ToString()),
                    Content = new StringContent(soapxml)
                    {
                        Headers =
                        {
                            ContentType = new MediaTypeHeaderValue("application/soap+xml")
                        }
                    }
                };
                using (var response = await client.SendAsync(request))
                {
                    // response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation(body);
                    return await Task.Run(() => Ok(body));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        #endregion Methods

        // PUT api/<EventBusController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<EventBusController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
