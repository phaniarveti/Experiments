using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis.Standard;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.ExamineHelpers;
using Umbraco.Web;
using UmbracoExamine;
using UmbracoExamine.DataServices;
using umbraco.BusinessLogic;
using System.Linq;

namespace Umbraco.Tests.PublishedContent
{
    /// <summary>
	/// Tests the typed extension methods on IPublishedContent using the DefaultPublishedMediaStore
	/// </summary>
	[TestFixture, RequiresSTA]
    public class PublishedMediaTests : PublishedContentTestBase
	{
		
		public override void Initialize()
		{
			base.Initialize();
            UmbracoSettings.ForceSafeAliases = true;
            UmbracoSettings.UmbracoLibraryCacheDuration = 1800;
            UmbracoSettings.ForceSafeAliases = true;	
		}
        
		public override void TearDown()
		{
			base.TearDown();
		}

		/// <summary>
		/// Shared with PublishMediaStoreTests
		/// </summary>
		/// <param name="id"></param>
		/// <param name="umbracoContext"></param>
		/// <returns></returns>
		internal static IPublishedContent GetNode(int id, UmbracoContext umbracoContext)
		{
			var ctx = umbracoContext;
			var mediaStore = new DefaultPublishedMediaStore();
			var doc = mediaStore.GetDocumentById(ctx, id);
			Assert.IsNotNull(doc);
			return doc;
		}

		private IPublishedContent GetNode(int id)
		{
			return GetNode(id, GetUmbracoContext("/test", 1234));
		}
           
		[Test]
	    public void Ensure_Children_Sorted_With_Examine()
	    {
            var newIndexFolder = new DirectoryInfo(Path.Combine("App_Data\\CWSIndexSetTest", Guid.NewGuid().ToString()));
            var indexInit = new IndexInitializer();
            var indexer = indexInit.GetUmbracoIndexer(newIndexFolder);
            indexer.RebuildIndex();

            var store = new DefaultPublishedMediaStore(
                indexInit.GetUmbracoSearcher(newIndexFolder),
                indexInit.GetUmbracoIndexer(newIndexFolder));

            //we are using the media.xml media to test the examine results implementation, see the media.xml file in the ExamineHelpers namespace
            var publishedMedia = store.GetDocumentById(GetUmbracoContext("/test", 1234), 1111);
	        var rootChildren = publishedMedia.Children().ToArray();
	        var currSort = 0;
            for (var i = 0; i < rootChildren.Count(); i++)
            {
                Assert.GreaterOrEqual(rootChildren[i].SortOrder, currSort);
                currSort = rootChildren[i].SortOrder;
            }

                

	    }

		
	    [Test]
	    public void Do_Not_Find_In_Recycle_Bin()
	    {
            var newIndexFolder = new DirectoryInfo(Path.Combine("App_Data\\CWSIndexSetTest", Guid.NewGuid().ToString()));
            var indexInit = new IndexInitializer();
            var indexer = indexInit.GetUmbracoIndexer(newIndexFolder);	        
            indexer.RebuildIndex();
            var searcher = indexInit.GetUmbracoSearcher(newIndexFolder);
            var store = new DefaultPublishedMediaStore(searcher, indexer);
	        var ctx = GetUmbracoContext("/test", 1234);

            //ensure it is found
            var publishedMedia = store.GetDocumentById(ctx, 3113);
	        Assert.IsNotNull(publishedMedia);

            //move item to recycle bin
            var newXml = XElement.Parse(@"<node id='3113' version='5b3e46ab-3e37-4cfa-ab70-014234b5bd33' parentID='-21' level='1' writerID='0' nodeType='1032' template='0' sortOrder='2' createDate='2010-05-19T17:32:46' updateDate='2010-05-19T17:32:46' nodeName='Another Umbraco Image' urlName='acnestressscrub' writerName='Administrator' nodeTypeAlias='Image' path='-1,-21,3113'>
					<data alias='umbracoFile'><![CDATA[/media/1234/blah.pdf]]></data>
					<data alias='umbracoWidth'>115</data>
					<data alias='umbracoHeight'>268</data>
					<data alias='umbracoBytes'>10726</data>
					<data alias='umbracoExtension'>jpg</data>
				</node>");
            indexer.ReIndexNode(newXml, "media");

            //ensure it still exists in the index (raw examine search)
            var criteria = searcher.CreateSearchCriteria();
            var filter = criteria.Id(3113);
            var found = searcher.Search(filter.Compile());
            Assert.IsNotNull(found);
            Assert.AreEqual(1, found.TotalItemCount);

            //ensure it does not show up in the published media store
            var recycledMedia = store.GetDocumentById(ctx, 3113);
            Assert.IsNull(recycledMedia);
	    }

	    [Test]
		public void Children_With_Examine()
		{
			var newIndexFolder = new DirectoryInfo(Path.Combine("App_Data\\CWSIndexSetTest", Guid.NewGuid().ToString()));
			var indexInit = new IndexInitializer();
			var indexer = indexInit.GetUmbracoIndexer(newIndexFolder);
			indexer.RebuildIndex();

            var store = new DefaultPublishedMediaStore(
                indexInit.GetUmbracoSearcher(newIndexFolder),
                indexInit.GetUmbracoIndexer(newIndexFolder));

			//we are using the media.xml media to test the examine results implementation, see the media.xml file in the ExamineHelpers namespace
			var publishedMedia = store.GetDocumentById(GetUmbracoContext("/test", 1234), 1111);
			var rootChildren = publishedMedia.Children();
			Assert.IsTrue(rootChildren.Select(x => x.Id).ContainsAll(new[] { 2222, 1113, 1114, 1115, 1116 }));

			var publishedChild1 = store.GetDocumentById(GetUmbracoContext("/test", 1234), 2222);
			var subChildren = publishedChild1.Children();
			Assert.IsTrue(subChildren.Select(x => x.Id).ContainsAll(new[] { 2112 }));
		}

		[Test]
		public void Descendants_With_Examine()
		{
			var newIndexFolder = new DirectoryInfo(Path.Combine("App_Data\\CWSIndexSetTest", Guid.NewGuid().ToString()));
			var indexInit = new IndexInitializer();
			var indexer = indexInit.GetUmbracoIndexer(newIndexFolder);
			indexer.RebuildIndex();

            var store = new DefaultPublishedMediaStore(
                indexInit.GetUmbracoSearcher(newIndexFolder),
                indexInit.GetUmbracoIndexer(newIndexFolder));

			//we are using the media.xml media to test the examine results implementation, see the media.xml file in the ExamineHelpers namespace
			var publishedMedia = store.GetDocumentById(GetUmbracoContext("/test", 1234), 1111);
			var rootDescendants = publishedMedia.Descendants();
			Assert.IsTrue(rootDescendants.Select(x => x.Id).ContainsAll(new[] { 2112, 2222, 1113, 1114, 1115, 1116 }));

			var publishedChild1 = store.GetDocumentById(GetUmbracoContext("/test", 1234), 2222);
			var subDescendants = publishedChild1.Descendants();
			Assert.IsTrue(subDescendants.Select(x => x.Id).ContainsAll(new[] { 2112, 3113 }));
		}

		[Test]
		public void DescendantsOrSelf_With_Examine()
		{
			var newIndexFolder = new DirectoryInfo(Path.Combine("App_Data\\CWSIndexSetTest", Guid.NewGuid().ToString()));
			var indexInit = new IndexInitializer();
			var indexer = indexInit.GetUmbracoIndexer(newIndexFolder);
			indexer.RebuildIndex();

            var store = new DefaultPublishedMediaStore(
                indexInit.GetUmbracoSearcher(newIndexFolder),
                indexInit.GetUmbracoIndexer(newIndexFolder));

			//we are using the media.xml media to test the examine results implementation, see the media.xml file in the ExamineHelpers namespace
			var publishedMedia = store.GetDocumentById(GetUmbracoContext("/test", 1234), 1111);
			var rootDescendants = publishedMedia.DescendantsOrSelf();
			Assert.IsTrue(rootDescendants.Select(x => x.Id).ContainsAll(new[] { 1111, 2112, 2222, 1113, 1114, 1115, 1116 }));

			var publishedChild1 = store.GetDocumentById(GetUmbracoContext("/test", 1234), 2222);
			var subDescendants = publishedChild1.DescendantsOrSelf();
			Assert.IsTrue(subDescendants.Select(x => x.Id).ContainsAll(new[] { 2222, 2112, 3113 }));
		}

		[Test]
		public void Ancestors_With_Examine()
		{
			var newIndexFolder = new DirectoryInfo(Path.Combine("App_Data\\CWSIndexSetTest", Guid.NewGuid().ToString()));
			var indexInit = new IndexInitializer();
			var indexer = indexInit.GetUmbracoIndexer(newIndexFolder);
			indexer.RebuildIndex();

            var store = new DefaultPublishedMediaStore(
                indexInit.GetUmbracoSearcher(newIndexFolder),
                indexInit.GetUmbracoIndexer(newIndexFolder));

			//we are using the media.xml media to test the examine results implementation, see the media.xml file in the ExamineHelpers namespace
			var publishedMedia = store.GetDocumentById(GetUmbracoContext("/test", 1234), 3113);
			var ancestors = publishedMedia.Ancestors();
			Assert.IsTrue(ancestors.Select(x => x.Id).ContainsAll(new[] { 2112, 2222, 1111 }));
		}

		[Test]
		public void AncestorsOrSelf_With_Examine()
		{
			var newIndexFolder = new DirectoryInfo(Path.Combine("App_Data\\CWSIndexSetTest", Guid.NewGuid().ToString()));
			var indexInit = new IndexInitializer();
			var indexer = indexInit.GetUmbracoIndexer(newIndexFolder);
			indexer.RebuildIndex();

            var store = new DefaultPublishedMediaStore(
                indexInit.GetUmbracoSearcher(newIndexFolder),
                indexInit.GetUmbracoIndexer(newIndexFolder));

			//we are using the media.xml media to test the examine results implementation, see the media.xml file in the ExamineHelpers namespace
			var publishedMedia = store.GetDocumentById(GetUmbracoContext("/test", 1234), 3113);
			var ancestors = publishedMedia.AncestorsOrSelf();
			Assert.IsTrue(ancestors.Select(x => x.Id).ContainsAll(new[] { 3113, 2112, 2222, 1111 }));
		}

		[Test]
		public void Children_Without_Examine()
		{
			var user = new User(0);
			var mType = global::umbraco.cms.businesslogic.media.MediaType.MakeNew(user, "TestMediaType");
			var mRoot = global::umbraco.cms.businesslogic.media.Media.MakeNew("MediaRoot", mType, user, -1);
			
			var mChild1 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child1", mType, user, mRoot.Id);
			var mChild2 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child2", mType, user, mRoot.Id);
			var mChild3 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child3", mType, user, mRoot.Id);

			var mSubChild1 = global::umbraco.cms.businesslogic.media.Media.MakeNew("SubChild1", mType, user, mChild1.Id);
			var mSubChild2 = global::umbraco.cms.businesslogic.media.Media.MakeNew("SubChild2", mType, user, mChild1.Id);
			var mSubChild3 = global::umbraco.cms.businesslogic.media.Media.MakeNew("SubChild3", mType, user, mChild1.Id);

			var publishedMedia = GetNode(mRoot.Id);
			var rootChildren = publishedMedia.Children();
			Assert.IsTrue(rootChildren.Select(x => x.Id).ContainsAll(new[] {mChild1.Id, mChild2.Id, mChild3.Id}));

			var publishedChild1 = GetNode(mChild1.Id);
			var subChildren = publishedChild1.Children();
			Assert.IsTrue(subChildren.Select(x => x.Id).ContainsAll(new[] { mSubChild1.Id, mSubChild2.Id, mSubChild3.Id }));
		}
        
		[Test]
		public void Descendants_Without_Examine()
		{
			var user = new User(0);
			var mType = global::umbraco.cms.businesslogic.media.MediaType.MakeNew(user, "TestMediaType");
			var mRoot = global::umbraco.cms.businesslogic.media.Media.MakeNew("MediaRoot", mType, user, -1);

			var mChild1 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child1", mType, user, mRoot.Id);
			var mChild2 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child2", mType, user, mRoot.Id);
			var mChild3 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child3", mType, user, mRoot.Id);

			var mSubChild1 = global::umbraco.cms.businesslogic.media.Media.MakeNew("SubChild1", mType, user, mChild1.Id);
			var mSubChild2 = global::umbraco.cms.businesslogic.media.Media.MakeNew("SubChild2", mType, user, mChild1.Id);
			var mSubChild3 = global::umbraco.cms.businesslogic.media.Media.MakeNew("SubChild3", mType, user, mChild1.Id);

			var publishedMedia = GetNode(mRoot.Id);
			var rootDescendants = publishedMedia.Descendants();
			Assert.IsTrue(rootDescendants.Select(x => x.Id).ContainsAll(new[] { mChild1.Id, mChild2.Id, mChild3.Id, mSubChild1.Id, mSubChild2.Id, mSubChild3.Id }));

			var publishedChild1 = GetNode(mChild1.Id);
			var subDescendants = publishedChild1.Descendants();
			Assert.IsTrue(subDescendants.Select(x => x.Id).ContainsAll(new[] { mSubChild1.Id, mSubChild2.Id, mSubChild3.Id }));
		}

		[Test]
		public void DescendantsOrSelf_Without_Examine()
		{
			var user = new User(0);
			var mType = global::umbraco.cms.businesslogic.media.MediaType.MakeNew(user, "TestMediaType");
			var mRoot = global::umbraco.cms.businesslogic.media.Media.MakeNew("MediaRoot", mType, user, -1);

			var mChild1 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child1", mType, user, mRoot.Id);
			var mChild2 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child2", mType, user, mRoot.Id);
			var mChild3 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child3", mType, user, mRoot.Id);

			var mSubChild1 = global::umbraco.cms.businesslogic.media.Media.MakeNew("SubChild1", mType, user, mChild1.Id);
			var mSubChild2 = global::umbraco.cms.businesslogic.media.Media.MakeNew("SubChild2", mType, user, mChild1.Id);
			var mSubChild3 = global::umbraco.cms.businesslogic.media.Media.MakeNew("SubChild3", mType, user, mChild1.Id);

			var publishedMedia = GetNode(mRoot.Id);
			var rootDescendantsOrSelf = publishedMedia.DescendantsOrSelf();
			Assert.IsTrue(rootDescendantsOrSelf.Select(x => x.Id).ContainsAll(
				new[] { mRoot.Id, mChild1.Id, mChild2.Id, mChild3.Id, mSubChild1.Id, mSubChild2.Id, mSubChild3.Id }));

			var publishedChild1 = GetNode(mChild1.Id);
			var subDescendantsOrSelf = publishedChild1.DescendantsOrSelf();
			Assert.IsTrue(subDescendantsOrSelf.Select(x => x.Id).ContainsAll(
				new[] { mChild1.Id, mSubChild1.Id, mSubChild2.Id, mSubChild3.Id }));
		}

		[Test]
		public void Parent_Without_Examine()
		{
			var user = new User(0);
			var mType = global::umbraco.cms.businesslogic.media.MediaType.MakeNew(user, "TestMediaType");
			var mRoot = global::umbraco.cms.businesslogic.media.Media.MakeNew("MediaRoot", mType, user, -1);

			var mChild1 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child1", mType, user, mRoot.Id);
			var mChild2 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child2", mType, user, mRoot.Id);
			var mChild3 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child3", mType, user, mRoot.Id);

			var mSubChild1 = global::umbraco.cms.businesslogic.media.Media.MakeNew("SubChild1", mType, user, mChild1.Id);
			var mSubChild2 = global::umbraco.cms.businesslogic.media.Media.MakeNew("SubChild2", mType, user, mChild1.Id);
			var mSubChild3 = global::umbraco.cms.businesslogic.media.Media.MakeNew("SubChild3", mType, user, mChild1.Id);

			var publishedRoot = GetNode(mRoot.Id);
			Assert.AreEqual(null, publishedRoot.Parent);

			var publishedChild1 = GetNode(mChild1.Id);
			Assert.AreEqual(mRoot.Id, publishedChild1.Parent.Id);

			var publishedSubChild1 = GetNode(mSubChild1.Id);
			Assert.AreEqual(mChild1.Id, publishedSubChild1.Parent.Id);
		}

		[Test]
		public void Ancestors_Without_Examine()
		{
			var user = new User(0);
			var mType = global::umbraco.cms.businesslogic.media.MediaType.MakeNew(user, "TestMediaType");
			var mRoot = global::umbraco.cms.businesslogic.media.Media.MakeNew("MediaRoot", mType, user, -1);

			var mChild1 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child1", mType, user, mRoot.Id);
			var mChild2 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child2", mType, user, mRoot.Id);
			var mChild3 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child3", mType, user, mRoot.Id);

			var mSubChild1 = global::umbraco.cms.businesslogic.media.Media.MakeNew("SubChild1", mType, user, mChild1.Id);
			var mSubChild2 = global::umbraco.cms.businesslogic.media.Media.MakeNew("SubChild2", mType, user, mChild1.Id);
			var mSubChild3 = global::umbraco.cms.businesslogic.media.Media.MakeNew("SubChild3", mType, user, mChild1.Id);

			var publishedSubChild1 = GetNode(mSubChild1.Id);
			Assert.IsTrue(publishedSubChild1.Ancestors().Select(x => x.Id).ContainsAll(new[] {mChild1.Id, mRoot.Id}));
		}

		[Test]
		public void AncestorsOrSelf_Without_Examine()
		{
			var user = new User(0);
			var mType = global::umbraco.cms.businesslogic.media.MediaType.MakeNew(user, "TestMediaType");
			var mRoot = global::umbraco.cms.businesslogic.media.Media.MakeNew("MediaRoot", mType, user, -1);

			var mChild1 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child1", mType, user, mRoot.Id);
			var mChild2 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child2", mType, user, mRoot.Id);
			var mChild3 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child3", mType, user, mRoot.Id);

			var mSubChild1 = global::umbraco.cms.businesslogic.media.Media.MakeNew("SubChild1", mType, user, mChild1.Id);
			var mSubChild2 = global::umbraco.cms.businesslogic.media.Media.MakeNew("SubChild2", mType, user, mChild1.Id);
			var mSubChild3 = global::umbraco.cms.businesslogic.media.Media.MakeNew("SubChild3", mType, user, mChild1.Id);

			var publishedSubChild1 = GetNode(mSubChild1.Id);
			Assert.IsTrue(publishedSubChild1.AncestorsOrSelf().Select(x => x.Id).ContainsAll(
				new[] { mSubChild1.Id, mChild1.Id, mRoot.Id }));
		}
	}

	
}