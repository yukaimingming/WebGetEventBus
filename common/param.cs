using System.Text;
using System.Xml;
using WebGetEventBus.models;

namespace WebGetEventBus.common
{
    public interface IGetxml
    {
        #region Methods

        /// <summary>
        ///  对xml节点进行换行，格式化对齐操作
        /// </summary>
        /// <param name="srcXml">
        /// </param>
        /// <returns>
        /// </returns>
        string GetXml(Root? root);

        #endregion Methods
    }

    public class strBuder : IGetxml
    {
        #region Fields

        private readonly IConfiguration Configuration;

        #endregion Fields

        #region Constructors

        public strBuder(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        #endregion Constructors

        #region Methods

        public static string FormatXml(string srcXml)
        {
            string formattedXml = IndentedFormat(IndentedFormat(srcXml).Replace("><", ">\r\n<"));
            return formattedXml;
        }

        public string GetXml(Root? str)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n");
            stringBuilder.Append("<soap12:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\r\n\t\t\t\t");
            stringBuilder.Append("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"\r\n\t\t\t\t");
            stringBuilder.Append("xmlns:soap12=\"http://www.w3.org/2003/05/soap-envelope\">\r\n\t");
            stringBuilder.Append("<soap12:Body>\r\n\t");
            stringBuilder.Append("<postsalescreate xmlns=\"http://tempurl.org\">\r\n\t\t\t");
            stringBuilder.Append("<astr_request>\r\n\t\t\t");
            stringBuilder.Append("<header>\r\n\t\t\t\t\t");
            stringBuilder.Append("<username>" + Configuration["header:username"] + "</username>\r\n\t\t\t\t\t");
            stringBuilder.Append("<password>" + Configuration["header:password"] + "</password>\r\n\t\t\t\t\t");
            stringBuilder.Append("<messagetype>" + Configuration["header:messagetype"] + "</messagetype>\r\n\t\t\t\t\t");
            stringBuilder.Append("<messageid>" + Configuration["header:messageid"] + "</messageid>\r\n\t\t\t\t\t");
            stringBuilder.Append("<version>" + Configuration["header:version"] + "</version>\r\n\t\t\t\t");
            stringBuilder.Append("</header>\r\n\t\t\t\t");
            stringBuilder.Append("<salestotal>\r\n\t\t\t\t\t");
            stringBuilder.Append("<txdate_yyyymmdd>" + str?.data?.main?.account_date?.Replace("-", "").Substring(0, 8) + "</txdate_yyyymmdd>\r\n\t\t\t\t\t");
            stringBuilder.Append("<txtime_hhmmss>" + str?.data?.main?.account_date?.Replace("-", "").Replace(":", "").Substring(8, 6) + "</txtime_hhmmss>\r\n\t\t\t\t\t");
            stringBuilder.Append("<mallid>" + Configuration["header:mallid"] + "</mallid>\r\n\t\t\t\t\t");
            stringBuilder.Append("<storecode>" + Configuration["header:storecode"] + "</storecode>\r\n\t\t\t\t\t");
            stringBuilder.Append("<tillid>01</tillid>\r\n\t\t\t\t\t");
            stringBuilder.Append("<txdocno>" + str!.data!.main!.x2_bill_id + "</txdocno>\r\n\t\t\t\t\t");

            foreach (var item in str!.data!.retail_clerks!)
            {
                stringBuilder.Append("<cashier>" + item.clerk_id + "</cashier>\r\n\t\t\t\t\t");
                stringBuilder.Append("<salesman>" + item.clerk_name + "</salesman>\r\n\t\t\t\t\t");
            }
            stringBuilder.Append("</salestotal>\r\n\t\t\t");
            stringBuilder.Append("<salesitems>\r\n\t\t\t\t\t");
            foreach (var item in str!.data!.subs!)
            {
                stringBuilder.Append("<salesitem>\r\n\t\t\t\t\t\t");
                stringBuilder.Append("<itemcode>" + Configuration["header:itemcode"] + "</itemcode>\r\n\t\t\t\t\t\t");//商品编号/货号
                stringBuilder.Append("<qty>" + item.nums + "</qty>\r\n\t\t\t\t\t\t");//数量退货时数量为负
                stringBuilder.Append("<netamount>" + item.billsub_ssum + "</netamount>\r\n\t\t\t\t\t");//净金额
                stringBuilder.Append("</salesitem>\r\n\t\t\t\t\t");
            }
            stringBuilder.Append("</salesitems>\r\n\t\t\t\t");
            stringBuilder.Append("<salestenders>\r\n\t\t\t\t\t");
            foreach (var item in str!.data!.retail_pays!)
            {
                stringBuilder.Append("<salestender>\r\n\t\t\t\t\t\t");

                switch (item.pay_type)
                {
                    case "CASH":
                        stringBuilder.Append("<tendercode>CH</tendercode>\r\n\t\t\t\t\t\t");
                        break;

                    case "BANKCARD":
                        stringBuilder.Append("<tendercode>CI</tendercode>\r\n\t\t\t\t\t\t");
                        break;

                    case "ZFBSK":
                        stringBuilder.Append("<tendercode>AP</tendercode>\r\n\t\t\t\t\t\t");
                        break;

                    case "WXSK":
                        stringBuilder.Append("<tendercode>WX</tendercode>\r\n\t\t\t\t\t\t");
                        break;

                    default:
                        stringBuilder.Append("<tendercode>OT</tendercode>\r\n\t\t\t\t\t\t");
                        break;
                }

                stringBuilder.Append("<payamount>" + item.sums + "</payamount>\r\n\t\t\t\t\t\t");//数量退货时数量为负
                stringBuilder.Append("<baseamount>" + item.sums + "</baseamount>\r\n\t\t\t\t\t");//净金额
                stringBuilder.Append("</salestender>\r\n\t\t\t\t\t");
            }
            stringBuilder.Append("</salestenders>\r\n\t\t\t");
            stringBuilder.Append("</astr_request>\r\n\t\t");
            stringBuilder.Append("</postsalescreate>\r\n\t");
            stringBuilder.Append("</soap12:Body>\r\n");
            stringBuilder.Append("</soap12:Envelope>");

            return stringBuilder.ToString();
        }

        /// <summary>
        ///  对XML字符串进行换行缩进，格式化
        /// </summary>
        /// <param name="xml">
        /// </param>
        /// <returns>
        /// </returns>
        private static string IndentedFormat(string xml)
        {
            string indentedText = string.Empty;
            try
            {
                XmlTextReader reader = new XmlTextReader(new StringReader(xml));
                reader.WhitespaceHandling = WhitespaceHandling.None;
                StringWriter indentedXmlWriter = new StringWriter();
                XmlTextWriter writer = CreateXmlTextWriter(indentedXmlWriter);
                writer.WriteNode(reader, false);
                writer.Flush();
                indentedText = indentedXmlWriter.ToString();
            }
            catch (Exception)
            {
                indentedText = xml;
            }
            return indentedText;
        }

        /// <summary>
        ///  写入四个缩进字符【空格】
        /// </summary>
        /// <returns>
        /// </returns>
        private static XmlTextWriter CreateXmlTextWriter(TextWriter textWriter)
        {
            XmlTextWriter writer = new XmlTextWriter(textWriter);

            //将Tab转化为4个空格
            bool convertTabsToSpaces = true;
            if (convertTabsToSpaces)
            {
                writer.Indentation = 4;
                writer.IndentChar = ' ';
            }
            else
            {
                writer.Indentation = 1;
                writer.IndentChar = '\t';
            }
            writer.Formatting = Formatting.Indented;
            return writer;
        }

        #endregion Methods
    }
}
