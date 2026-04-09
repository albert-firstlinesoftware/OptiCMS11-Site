using System;
using System.ComponentModel.DataAnnotations;
using AlloyTemplates.Models.Blocks;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace AlloyTemplates.Models.Pages
{
    /// <summary>
    /// Used to publish events such as webinars, conferences, and meetups
    /// </summary>
    [SiteContentType(
        GUID = "D4A3E82F-1C5B-4A7E-9B3D-6F8E2C4A1D09",
        GroupName = Global.GroupNames.Specialized)]
    [SiteImageUrl(Global.StaticGraphicsFolderPath + "page-type-thumbnail-standard.png")]
    [AvailableContentTypes(
        Availability = Availability.Specific,
        IncludeOn = new[] { typeof(StartPage), typeof(ContainerPage) })]
    public class EventPage : StandardPage, IHasRelatedContent
    {
        [Display(
            Name = "Event start",
            GroupName = SystemTabNames.Content,
            Order = 220)]
        [Required]
        public virtual DateTime EventStartDate { get; set; }

        [Display(
            Name = "Event end",
            GroupName = SystemTabNames.Content,
            Order = 225)]
        public virtual DateTime EventEndDate { get; set; }

        [Display(
            Name = "Location",
            GroupName = SystemTabNames.Content,
            Order = 230)]
        [CultureSpecific]
        public virtual string Location { get; set; }

        [Display(
            Name = "Registration link",
            GroupName = SystemTabNames.Content,
            Order = 240)]
        public virtual Url RegistrationLink { get; set; }

        [Display(
            Name = "Is virtual event",
            GroupName = SystemTabNames.Content,
            Order = 250)]
        public virtual bool IsVirtualEvent { get; set; }

        [Display(
            Name = "Related content",
            GroupName = SystemTabNames.Content,
            Order = 330)]
        [CultureSpecific]
        [AllowedTypes(new[] { typeof(IContentData) }, new[] { typeof(JumbotronBlock) })]
        public virtual ContentArea RelatedContentArea { get; set; }

        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            EventStartDate = DateTime.Today.AddDays(7);
            IsVirtualEvent = false;
        }
    }
}
