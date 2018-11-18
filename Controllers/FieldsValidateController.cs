﻿using System;
using System.Web.Http;
using SiteServer.Plugin;
using SS.GovApplication.Core;
using SS.GovApplication.Core.Provider;
using SS.GovApplication.Core.Utils;

namespace SS.GovApplication.Controllers
{
    [RoutePrefix("fields/validate")]
    public class FieldsValidateController : ApiController
    {
        private const string Route = "";

        [HttpGet, Route(Route)]
        public IHttpActionResult Get()
        {
            try
            {
                var request = Context.GetCurrentRequest();
                var siteId = request.GetQueryInt("siteId");
                if (!request.IsAdminLoggin || !request.AdminPermissions.HasSitePermissions(siteId, Utils.PluginId)) return Unauthorized();

                var fieldId = request.GetQueryInt("fieldId");
                var fieldInfo = FieldManager.GetFieldInfo(fieldId);

                var veeValidate = string.Empty;
                if (fieldInfo != null)
                {
                    veeValidate = fieldInfo.Validate;
                }

                return Ok(new
                {
                    Value = veeValidate
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost, Route(Route)]
        public IHttpActionResult Submit()
        {
            try
            {
                var request = Context.GetCurrentRequest();
                var siteId = request.GetQueryInt("siteId");
                if (!request.IsAdminLoggin || !request.AdminPermissions.HasSitePermissions(siteId, Utils.PluginId)) return Unauthorized();

                var fieldId = request.GetPostInt("fieldId");
                var value = request.GetPostString("value");

                var fieldInfo = FieldManager.GetFieldInfo(fieldId);
                fieldInfo.Validate = value;

                FieldDao.Update(fieldInfo, false);

                return Ok(new{});
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
