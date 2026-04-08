using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Framework.Web;
using EPiServer.Search;
using EPiServer.Web;
using EPiServer.Web.Mvc.Html;
using EPiServer.Web.Routing;
using AlloyTemplates.Business;
using AlloyTemplates.Models.Pages;
using AlloyTemplates.Models.ViewModels;

namespace AlloyTemplates.Controllers
{
    /// <summary>
    /// Concrete controller that handles all page types that don't have their own specific controllers.
    /// </summary>
    /// <remarks>
    /// Note that as the view file name is hard coded it won't work with DisplayModes (ie Index.mobile.cshtml).
    /// For page types requiring such views add specific controllers for them. Alterntively the Index action
    /// could be modified to set ControllerContext.RouteData.Values["controller"] to type name of the currentPage
    /// argument. That may however have side effects.
    /// </remarks>
    [TemplateDescriptor(Inherited = true)]
    public class DefaultPageController : PageControllerBase<SitePageData>
    {
        private const int MaxResults = 40;
        private readonly IContentLoader _contentLoader;
        private readonly SearchService _searchService;
        private readonly ContentSearchHandler _contentSearchHandler;
        private readonly UrlResolver _urlResolver;
        private readonly TemplateResolver _templateResolver;

        public DefaultPageController(
            IContentLoader contentLoader,
            SearchService searchService,
            ContentSearchHandler contentSearchHandler,
            TemplateResolver templateResolver,
            UrlResolver urlResolver)
        {
            _contentLoader = contentLoader;
            _searchService = searchService;
            _contentSearchHandler = contentSearchHandler;
            _templateResolver = templateResolver;
            _urlResolver = urlResolver;
        }

        public ViewResult Index(SitePageData currentPage, string q)
        {
            // Check if this page is configured as the search page
            var startPage = _contentLoader.Get<StartPage>(SiteDefinition.Current.StartPage);
            if (!ContentReference.IsNullOrEmpty(startPage.SearchPageLink)
                && currentPage.ContentLink.CompareToIgnoreWorkID(startPage.SearchPageLink))
            {
                return SearchIndex(currentPage, q);
            }

            var model = CreateModel(currentPage);
            return View(string.Format("~/Views/{0}/Index.cshtml", currentPage.GetOriginalType().Name), model);
        }

        private ViewResult SearchIndex(SitePageData currentPage, string q)
        {
            var model = new SearchContentModel(currentPage)
            {
                SearchServiceDisabled = !_searchService.IsActive,
                SearchedQuery = q
            };

            if (!string.IsNullOrWhiteSpace(q) && _searchService.IsActive)
            {
                var hits = Search(q.Trim(),
                    new[] { SiteDefinition.Current.StartPage, SiteDefinition.Current.GlobalAssetsRoot, SiteDefinition.Current.SiteAssetsRoot },
                    ControllerContext.HttpContext,
                    currentPage.Language?.Name).ToList();
                model.Hits = hits;
                model.NumberOfHits = hits.Count;
            }

            return View("~/Views/SearchPage/Index.cshtml", model);
        }

        private IEnumerable<SearchContentModel.SearchHit> Search(string searchText, IEnumerable<ContentReference> searchRoots, System.Web.HttpContextBase context, string languageBranch)
        {
            var searchResults = _searchService.Search(searchText, searchRoots, context, languageBranch, MaxResults);
            return searchResults.IndexResponseItems.SelectMany(CreateHitModel);
        }

        private IEnumerable<SearchContentModel.SearchHit> CreateHitModel(IndexResponseItem responseItem)
        {
            var content = _contentSearchHandler.GetContent<IContent>(responseItem);
            if (content != null && HasTemplate(content) && IsPublished(content as IVersionable))
            {
                yield return new SearchContentModel.SearchHit
                {
                    Title = content.Name,
                    Url = _urlResolver.GetUrl(content.ContentLink),
                    Excerpt = content is SitePageData ? ((SitePageData)content).TeaserText : string.Empty
                };
            }
        }

        private bool HasTemplate(IContent content)
        {
            return _templateResolver.HasTemplate(content, TemplateTypeCategories.Page);
        }

        private bool IsPublished(IVersionable content)
        {
            if (content == null)
                return true;
            return content.Status.HasFlag(VersionStatus.Published);
        }

        /// <summary>
        /// Creates a PageViewModel where the type parameter is the type of the page.
        /// </summary>
        /// <remarks>
        /// Used to create models of a specific type without the calling method having to know that type.
        /// </remarks>
        private static IPageViewModel<SitePageData> CreateModel(SitePageData page)
        {
            var type = typeof(PageViewModel<>).MakeGenericType(page.GetOriginalType());
            return Activator.CreateInstance(type, page) as IPageViewModel<SitePageData>;
        }
    }
}
