using ChilliSource.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using ChilliSource.Cloud.Core;

#if NET_4X
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Mvc.Html;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
#endif
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    public class TemplateOptions
    {
        public static Func<IFieldTemplateLayout> DefaultFieldTemplateLayout { get; set; }
        public static Func<IFieldTemplateOptions> DefaultFieldTemplateOptions { get; set; }

        public TemplateOptions()
        {
            Initialize();
        }

        IFieldTemplateLayout _template;
        public IFieldTemplateLayout Template { get { return _template; } set { _template = value ?? throw new ArgumentNullException(nameof(Template)); } }

        public string Label { get; set; }

        public string HelpText { get; set; }

        public int? FieldColumnSize { get; set; }

        public FieldTemplateSize? FieldSize { get; set; }

        public bool? IsMandatory { get; set; }

        /// <summary>
        /// Attributes merged into the outer tag of the template
        /// </summary>
        public object HtmlAttributes { get; set; }

        protected void Initialize()
        {
            Template = DefaultFieldTemplateLayout?.Invoke() ??
                        throw new ApplicationException("DefaultFieldTemplateLayout needs to be set via TemplateOptions.DefaultFieldTemplateLayout");
        }

        internal static IFieldTemplateOptions CreateDefaultFieldTemplateOptions()
        {
            return DefaultFieldTemplateOptions?.Invoke() ??
                        throw new ApplicationException("DefaultFieldTemplateOptions needs to be set via TemplateOptions.DefaultFieldTemplateOptions");
        }
    }

    //Marker interface
    public interface IFieldTemplateLayout: ITemplatePathProvider
    {
    }

    public class FieldTemplateLayout : IFieldTemplateLayout
    {
        public FieldTemplateLayout(string viewPath)
        {
            if (String.IsNullOrEmpty(viewPath))
                throw new ArgumentException(nameof(viewPath));

            this.ViewPath = viewPath;
        }

        public virtual string ViewPath { get; }

        public override bool Equals(object obj)
        {
            var cast = obj as IFieldTemplateLayout;
            return (cast != null) && cast.ViewPath == this.ViewPath;
        }

        public override int GetHashCode()
        {
            return this.ViewPath?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return this.ViewPath;
        }
    }

    public class FieldTemplateModel
    {
        public const string InnerTemplateMarker = "###InnerTemplate###";

        public static FieldTemplateSize DefaultSize { get; set; } = FieldTemplateSize.Medium;

        public FieldTemplateModel() { }

        public string ModelName { get; set; }

        public string DisplayName { get; set; }

        public bool IsMandatory { get; set; }

        public string HelpText { get; set; }

        public int? FieldColumnSize { get; set; }

        public FieldTemplateSize? FieldSize { get; set; }

        public RouteValueDictionary HtmlAttributes { get; set; }

        public static string GetFieldSize(FieldTemplateSize size)
        {
            switch (size)
            {
                case FieldTemplateSize.ExtraSmall: return "col-xxl-1 col-md-2 col-sm-3 col-xs-4";
                case FieldTemplateSize.Small: return "col-xxl-2 col-md-3 col-sm-4 col-xs-6";
                case FieldTemplateSize.Medium: return "col-xxl-4 col-xl-6 col-sm-8";
                case FieldTemplateSize.Large: return "col-xxl-8 col-xl-10";
                case FieldTemplateSize.ExtraLarge: return "col-xxl-10";
            }
            return "";
        }

        public string GetFieldSize()
        {
            return GetFieldSize(this.FieldSize.GetValueOrDefault(DefaultSize));            
        }
    }

    public enum FieldTemplateSize
    {
        ExtraSmall,
        Small,
        Medium,
        Large,
        ExtraLarge  //Full-width
    }

    public interface IFieldInnerTemplateModel
    {
        string Id { get; set; }

        string Name { get; set; }

        object Value { get; set; }

        string DisplayName { get; set; }

        FieldInnerTemplateMetadata InnerMetadata { get; set; }

        RouteValueDictionary HtmlAttributes { get; set; }

        IFieldInnerTemplateModel<TOptionsNew> UseOptions<TOptionsNew>(TOptionsNew options) where TOptionsNew : class, IFieldTemplateOptions;
        IFieldTemplateOptions GetOptions();
    }

    public class FieldInnerTemplateMetadata
    {
        public FieldInnerTemplateMetadata(ModelMetadata modelMetadata, object modelValue, MemberExpression memberExpression, Type memberType)
        {
            this.ModelMetadata = modelMetadata;
            this.ModelValue = modelValue;
            this.MemberExpression = memberExpression;

            this.MemberType = memberType;
            this.MemberUnderlyingType = modelMetadata.IsNullableValueType ? Nullable.GetUnderlyingType(memberType) : memberType;
        }

        public ModelMetadata ModelMetadata { get; }
        public object ModelValue { get; }
        public MemberExpression MemberExpression { get; }

        public Type MemberType { get; }
        public Type MemberUnderlyingType { get; }
    }

    public interface IFieldInnerTemplateModel<TOptions> : IFieldInnerTemplateModel
        where TOptions : IFieldTemplateOptions
    {
        TOptions Options { get; }
    }

    internal class FieldInnerTemplateModel<TOptions> : IFieldInnerTemplateModel<TOptions>
        where TOptions : IFieldTemplateOptions
    {
        FieldInnerTemplateModel _templateModel;

        public FieldInnerTemplateModel(FieldInnerTemplateModel templateModel, TOptions options)
        {
            _templateModel = templateModel ?? new FieldInnerTemplateModel();

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            this.Options = options;
        }
        public TOptions Options { get; }
        public string Id { get => _templateModel.Id; set => _templateModel.Id = value; }
        public string Name { get => _templateModel.Name; set => _templateModel.Name = value; }
        public object Value { get => _templateModel.Value; set => _templateModel.Value = value; }
        public string DisplayName { get => _templateModel.DisplayName; set => _templateModel.DisplayName = value; }
        public RouteValueDictionary HtmlAttributes { get => _templateModel.HtmlAttributes; set => _templateModel.HtmlAttributes = value; }
        public FieldInnerTemplateMetadata InnerMetadata { get => _templateModel.InnerMetadata; set => _templateModel.InnerMetadata = value; }

        public IFieldInnerTemplateModel<TOptionsNew> UseOptions<TOptionsNew>(TOptionsNew options)
            where TOptionsNew : class, IFieldTemplateOptions
        {
            if (typeof(TOptions) == typeof(TOptionsNew) && Object.ReferenceEquals(options, this.Options))
                return (IFieldInnerTemplateModel<TOptionsNew>)this;

            return new FieldInnerTemplateModel<TOptionsNew>(_templateModel, options: options);
        }

        public IFieldTemplateOptions GetOptions()
        {
            return this.Options;
        }
    }

    public class FieldInnerTemplateModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public object Value { get; set; }

        public FieldInnerTemplateMetadata InnerMetadata { get; set; }

        public string DisplayName { get; set; }

        public RouteValueDictionary HtmlAttributes { get; set; }

        private IFieldInnerTemplateModel<TOptionsNew> UseOptionsCast<TOptionsNew>(TOptionsNew options)
            where TOptionsNew : class, IFieldTemplateOptions
        {
            return new FieldInnerTemplateModel<TOptionsNew>(this, options);
        }

        public IFieldInnerTemplateModel UseOptions(IFieldTemplateOptions fieldOptions)
        {
            if (fieldOptions == null)
                throw new ArgumentNullException(nameof(fieldOptions));

            var cast = this.UseOptionsCast((dynamic)fieldOptions);
            return (IFieldInnerTemplateModel)cast;
        }
    }

    public interface IFieldTemplateOptions
    {
        object HtmlAttributes { get; set; }

#if NET_4X
        IFieldInnerTemplateModel CreateFieldInnerTemplateModel<TModel, TValue>(HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression);               
#else
        IFieldInnerTemplateModel CreateFieldInnerTemplateModel<TModel, TValue>(IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression);
#endif

        IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel);

        IFieldInnerTemplateModel PostProcessInnerField(IFieldInnerTemplateModel templateModel);

#if NET_4X
        IHtmlContent Render(HtmlHelper htmlHelper, IFieldInnerTemplateModel templateModel);
#else
        Task<IHtmlContent> RenderAsync(IHtmlHelper htmlHelper, IFieldInnerTemplateModel templateModel);
#endif
    }

    public abstract class FieldTemplateOptionsBase : IFieldTemplateOptions
    {
        protected FieldTemplateOptionsBase()
        {
            AutoWireUpJavascript = true;
        }

        protected FieldTemplateOptionsBase(FieldTemplateOptionsBase other)
        {
            this.HtmlAttributes = other.HtmlAttributes;
            this.AutoWireUpJavascript = other.AutoWireUpJavascript;
            this.Roles = other.Roles;
        }

        public object HtmlAttributes { get; set; }

        public bool AutoWireUpJavascript { get; set; }

        public string[] Roles { get; set; }
#if NET_4X
        public virtual IFieldInnerTemplateModel CreateFieldInnerTemplateModel<TModel, TValue>(HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            object model = metadata.Model;
#else
        public virtual IFieldInnerTemplateModel CreateFieldInnerTemplateModel<TModel, TValue>(IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
#if NETSTANDARD2_0

            var explorer = ExpressionMetadataProvider.FromLambdaExpression(expression, html.ViewData, html.MetadataProvider);
#else
            var expressionProvider = new ModelExpressionProvider(html.MetadataProvider);
            var explorer = expressionProvider.CreateModelExpression(html.ViewData, expression).ModelExplorer;
#endif
            ModelMetadata metadata = explorer.Metadata;
            object model = explorer.Model;
#endif

            var member = expression.Body as MemberExpression;
            var name = html.NameFor(expression).ToString();

            string attemptedValue = null;
            if (html.ViewContext.ViewData.ModelState.ContainsKey(name))
            {
                var kvp = html.ViewContext.ViewData.ModelState[name];
#if NET_4X
                attemptedValue = kvp.Value?.AttemptedValue;
#else
                attemptedValue = kvp.AttemptedValue;
#endif
            }

            var attributes = RouteValueDictionaryHelper.CreateFromHtmlAttributes(this.HtmlAttributes);
            string dataId = null;
            if (attributes.ContainsKey("Id"))
            {
                dataId = attributes["Id"].ToString();
                attributes.Remove("Id");
            }
            dataId = dataId ?? html.IdFor(expression).ToString();

#if NET_4X
            var validationAttributes = new RouteValueDictionary(html.GetUnobtrusiveValidationAttributes(name, metadata));
#else
            var validator = html.ViewContext.HttpContext.RequestServices.GetService<ValidationHtmlAttributeProvider>();
            var validationAttributes = new Dictionary<string, string>();
            validator?.AddAndTrackValidationAttributes(html.ViewContext, explorer, name, validationAttributes);
#endif
            attributes.Merge(validationAttributes);

            var innerMetadata = new FieldInnerTemplateMetadata(metadata, model, member, typeof(TValue));
            var data = new FieldInnerTemplateModel
            {
                Id = dataId,
                Name = name,
                Value = String.IsNullOrEmpty(attemptedValue) || (model != null && attemptedValue == model.ToString()) ? model : attemptedValue,
                DisplayName = html.GetLabelTextFor(expression),
                HtmlAttributes = attributes,
                InnerMetadata = innerMetadata
            };

            var templateModel = data.UseOptions(this);

            return templateModel;
        }

        public virtual IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            return templateModel;
        }

        public virtual IFieldInnerTemplateModel PostProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            if (templateModel.HtmlAttributes.ContainsKey("Name"))
            {
                templateModel.Name = templateModel.HtmlAttributes["Name"].ToString();
            }

            return templateModel;
        }

#if NET_4X
        public virtual IHtmlContent Render(HtmlHelper htmlHelper, IFieldInnerTemplateModel templateModel)
        {
            var partialPath = this.GetViewPath();
            return htmlHelper.Partial(partialPath, templateModel).AsHtmlContent();
        }
#else
        public virtual Task<IHtmlContent> RenderAsync(IHtmlHelper htmlHelper, IFieldInnerTemplateModel templateModel)
        {
            var partialPath = this.GetViewPath();
            return htmlHelper.PartialAsync(partialPath, templateModel);
        }
#endif

        public abstract string GetViewPath();
    }

}
