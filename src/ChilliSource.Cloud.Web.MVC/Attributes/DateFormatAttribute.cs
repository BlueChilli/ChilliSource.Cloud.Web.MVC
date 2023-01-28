using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>    
    /// Allows configuring what parts of a DateTime field should be displayed (Day, Month, Year, Hours, Minutes).
    /// </summary>
    public class DateFormatAttribute : Attribute, IMetadataAware
    {
        /// <summary>
        /// Specifies whether the Day field should be displayed. Defaults to true.
        /// </summary>
        public bool ShowDay { get; set; }
        /// <summary>
        /// Specifies whether the Month field should be displayed. Defaults to true.
        /// </summary>
        public bool ShowMonth { get; set; }
        /// <summary>
        /// Specifies whether the Year field should be displayed. Defaults to true.
        /// </summary>
        public bool ShowYear { get; set; }
        /// <summary>
        /// Specifies whether Hours should be displayed. Default to False.
        /// </summary>
        public bool ShowHour { get; set; }
        /// <summary>
        /// Specifies whether Minutes should be displayed. Default to False.
        /// </summary>
        public bool ShowMinute { get; set; }

        /// <summary>
        /// Specifies whether Helptext which reflects the date entered should be displayed. Default to True.
        /// </summary>
        public bool ShowHelpText { get; set; }

        public DateFormatAttribute()
        {
            ShowHelpText = true;

            ShowDay = true;
            ShowMonth = true;
            ShowYear = true;

            ShowHour = false;
            ShowMinute = false;
        }

#if NET_4X
        public void OnMetadataCreated(ModelMetadata metadata)
#else
        public void GetDisplayMetadata(DisplayMetadataProviderContext metadata)
#endif
        {
            metadata.AdditionalValues()["DateShowDay"] = ShowDay;
            metadata.AdditionalValues()["DateShowMonth"] = ShowMonth;
            metadata.AdditionalValues()["DateShowYear"] = ShowYear;
            metadata.AdditionalValues()["DateShowHour"] = ShowHour;
            metadata.AdditionalValues()["DateShowMinute"] = ShowMinute;
            metadata.AdditionalValues()["DateShowHelpText"] = ShowHelpText;
        }
    }
}