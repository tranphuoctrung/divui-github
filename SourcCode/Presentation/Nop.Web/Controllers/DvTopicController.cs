using Nop.Core.Caching;
using Nop.Services.Customers;
using Nop.Web.Infrastructure.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nop.Web.Controllers
{
    public partial class TopicController
    {
        // GET: DvTopic
        [ChildActionOnly]
        public ActionResult HomePageTopicBlock(string systemName, string classItem, string classTitle, string classDesc)
        {
            var cacheKey = string.Format(ModelCacheEventConsumer.TOPIC_MODEL_BY_SYSTEMNAME_KEY,
                systemName,
                _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()));
            var cacheModel = _cacheManager.Get(cacheKey, () =>
            {
                //load by store
                var topic = _topicService.GetTopicBySystemName(systemName, _storeContext.CurrentStore.Id);
                if (topic == null)
                    return null;
                //Store mapping
                if (!_storeMappingService.Authorize(topic))
                    return null;
                //ACL (access control list)
                if (!_aclService.Authorize(topic))
                    return null;
                return PrepareTopicModel(topic);
            });

            if (cacheModel == null)
                return Content("");

            ViewBag.classItem = classItem;
            ViewBag.classTitle = classTitle;
            ViewBag.classDesc = classDesc;

            return PartialView(cacheModel);
        }
        
    }
}