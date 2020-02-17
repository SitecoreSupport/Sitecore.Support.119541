using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;
using Sitecore.Pipelines.Save;
using Sitecore.Web.UI.Sheer;
using System.Collections.Generic;
using System.Linq;
using static Sitecore.Pipelines.Save.SaveArgs;
using System;


namespace Sitecore.Support
{
    public class ValidateFields
    {
        public void Process(SaveArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.IsNotNull(args.Items, "args.Items");


            SaveArgs.SaveItem[] items = args.Items;
            foreach (SaveArgs.SaveItem saveItem in items)
            {
                Item item = Client.ContentDatabase.GetItem(saveItem.ID, saveItem.Language);
                if (item == null || item.Paths.IsMasterPart || StandardValuesManager.IsStandardValuesHolder(item))
                {
                    continue;
                }

                SaveArgs.SaveField[] fields = (from x in saveItem.Fields orderby Client.ContentDatabase.GetItem(x.ID).Appearance.Sortorder select x).ToArray<SaveArgs.SaveField>();
                foreach (SaveArgs.SaveField saveField in fields)
                {
                    Field field = item.Fields[saveField.ID];
                    string fieldRegexValidationError = FieldUtil.GetFieldRegexValidationError(field, saveField.Value);
                    if (!string.IsNullOrEmpty(fieldRegexValidationError))
                    {
                        if (args.HasSheerUI)
                        {
                            SheerResponse.Alert(fieldRegexValidationError);
                            SheerResponse.SetReturnValue("failed");
                        }
                        args.AbortPipeline();
                        return;
                    }
                }
            }
        }
    }
}