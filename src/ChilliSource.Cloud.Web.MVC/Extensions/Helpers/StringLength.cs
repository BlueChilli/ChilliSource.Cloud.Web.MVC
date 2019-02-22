
using ChilliSource.Core.Extensions; using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
#if NET_4X
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.DataProtection;
#endif
using System.Web.Mvc.Html;
using System.Web.Routing;


namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
 
        /// <summary>
        /// Resolve attributes on a member related to string properties. Eg MaxLength, MinLength, StringLength, CharacterLeft. 
        /// Used in FieldTemplateInnerFor.
        /// </summary>
        public static void ResolveStringLength(MemberExpression member, IDictionary<string, object> attributes)
        {
            var maxLength = member.Member.GetAttribute<MaxLengthAttribute>(false);
            if (maxLength != null && maxLength.Length > 0)
            {
                attributes["maxlength"] = maxLength.Length;
            }

            var minLength = member.Member.GetAttribute<MinLengthAttribute>(false);
            if (minLength != null && minLength.Length > 0)
            {
                attributes["minlength"] = minLength.Length;
            }

            // StringLength overrides MinLength and MaxLength (you are not supposed to use both anyway):
            var stringLength = member.Member.GetAttribute<StringLengthAttribute>(false);
            if (stringLength != null)
            {
                if (stringLength.MaximumLength > 0) attributes["maxlength"] = stringLength.MaximumLength;
                if (stringLength.MinimumLength > 0) attributes["minlength"] = stringLength.MinimumLength;
            }

            //CharactersLeft is always used in combination with StringLength
            var charactersLeft = member.Member.GetAttribute<CharactersLeftAttribute>(false);
            if (charactersLeft != null)
            {
                attributes["charactersleft"] = true;
            }
        }
    }
}