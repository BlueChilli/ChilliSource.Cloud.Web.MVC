using System.Text;

#if NET_4X
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.DataProtection;
#endif

namespace ChilliSource.Cloud.Web.MVC.Misc
{
    /// <summary>
    /// Contains extension methods for System.Web.Mvc.HtmlHelper.
    /// </summary>
    public static class BlueChilliMotifHtmlHelper
    {
        private static readonly IHtmlContent bcHtmlContent = MvcHtmlStringCompatibility.Empty()
            .AppendLine("<!--")
            .AppendLine("                                                                                 +")
            .AppendLine("                                                                                 +")
            .AppendLine("                                                                                 @")
            .AppendLine("                                                                                 @")
            .AppendLine("                                                                                 @")
            .AppendLine("    BBBBBBb    LLl                      cCCCc   hHH       II  LLl lLL  II        @")
            .AppendLine("    BBBBBBBBb  LLl                    cCCCCCCC  hHH       II  LLl lLL  II       #@")
            .AppendLine("    BBB   bBB  LLl                    CCC  cCCc hHH           LLl lLL          @@@")
            .AppendLine("    BBB    BB  LLl uUU   UU   eEEEe  cCC    cCC hHHhHHH   II  LLl lLL  II     ::::.")
            .AppendLine("    BBB   bBB  LLl uUU   UU  EEEEEEe CCC        hHHHHHHH  II  LLl lLL  II     ::::")
            .AppendLine("    BBBBBBBB   LLl uUU   UU eEE   EE CCc        hHH  hHH  II  LLl lLL  II     ::::")
            .AppendLine("    BBBbbbBBB  LLl uUU   UU EEeeeeEE CCc        hHH   HH  II  LLl lLL  II     :::,")
            .AppendLine("    BBB    BBb LLl uUU   UU EEEEEEEE CCc    ccc hHH   HH  II  LLl lLL  II     :::")
            .AppendLine("    BBB    BBb LLl uUU  uUU EEe      cCC    CCC hHH   HH  II  LLl lLL  II     ::`")
            .AppendLine("    BBBbbbBBB  LLl uUU  UUU eEE  eEE  CCCc cCCc hHH   HH  II  LLl lLL  II     ::,")
            .AppendLine("    BBBBBBBBb  LLl  UUUUUUU  eEEEEEe  cCCCCCCc  hHH   HH  II  LLl lLL  II     ::")
            .AppendLine("    bbbbbbbb   lll   uuuuuu   eEEee     cCCC    hhh   hh  ii  lll lLL  ii     ::")
            .AppendLine("                                                                              ::")
            .AppendLine("                                                                              :")
            .AppendLine("    -->");

        /// <summary>
        /// Returns HTML string for BlueChilli ASCII code.
        /// </summary>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <returns>An HTML-encoded string.</returns>
#if NET_4X
        public static IHtmlContent BlueChilliAsciiMotif(this HtmlHelper html)
#else
        public static IHtmlContent BlueChilliAsciiMotif(this IHtmlHelper html)
#endif
        {
            return bcHtmlContent;
        }
    }
}