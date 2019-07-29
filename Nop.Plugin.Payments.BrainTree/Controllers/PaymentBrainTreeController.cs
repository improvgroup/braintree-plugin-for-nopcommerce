﻿using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.BrainTree.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.BrainTree.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class PaymentBrainTreeController : BasePaymentController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public PaymentBrainTreeController(ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods
        
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var brainTreePaymentSettings = _settingService.LoadSetting<BrainTreePaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                UseSandBox = brainTreePaymentSettings.UseSandBox,
                PublicKey = brainTreePaymentSettings.PublicKey,
                PrivateKey = brainTreePaymentSettings.PrivateKey,
                MerchantId = brainTreePaymentSettings.MerchantId,
                AdditionalFee = brainTreePaymentSettings.AdditionalFee,
                AdditionalFeePercentage = brainTreePaymentSettings.AdditionalFeePercentage
            };

            if (storeScope > 0)
            {
                model.UseSandbox_OverrideForStore = _settingService.SettingExists(brainTreePaymentSettings, x => x.UseSandBox, storeScope);
                model.PublicKey_OverrideForStore = _settingService.SettingExists(brainTreePaymentSettings, x => x.PublicKey, storeScope);
                model.PrivateKey_OverrideForStore = _settingService.SettingExists(brainTreePaymentSettings, x => x.PrivateKey, storeScope);
                model.MerchantId_OverrideForStore = _settingService.SettingExists(brainTreePaymentSettings, x => x.MerchantId, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(brainTreePaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(brainTreePaymentSettings, x => x.AdditionalFeePercentage, storeScope);
            }

            return View("~/Plugins/Payments.BrainTree/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var brainTreePaymentSettings = _settingService.LoadSetting<BrainTreePaymentSettings>(storeScope);

            //save settings
            brainTreePaymentSettings.UseSandBox = model.UseSandBox;
            brainTreePaymentSettings.PublicKey = model.PublicKey;
            brainTreePaymentSettings.PrivateKey = model.PrivateKey;
            brainTreePaymentSettings.MerchantId = model.MerchantId;
            brainTreePaymentSettings.AdditionalFee = model.AdditionalFee;
            brainTreePaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(brainTreePaymentSettings, x => x.UseSandBox, model.UseSandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(brainTreePaymentSettings, x => x.PublicKey, model.PublicKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(brainTreePaymentSettings, x => x.PrivateKey, model.PrivateKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(brainTreePaymentSettings, x => x.MerchantId, model.MerchantId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(brainTreePaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(brainTreePaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        #endregion
    }
}