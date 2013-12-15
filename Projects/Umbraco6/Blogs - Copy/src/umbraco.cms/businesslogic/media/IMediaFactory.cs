﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.BusinessLogic;

namespace umbraco.cms.businesslogic.media
{
    public interface IMediaFactory
    {
        List<string> Extensions { get; }
        int Priority { get; }

        bool CanHandleMedia(int parentNodeId, PostedMediaFile postedFile, User user);

        Media HandleMedia(int parentNodeId, PostedMediaFile postedFile, User user);

        [Obsolete("Use HandleMedia(int, PostedMediaFile, User) and set the ReplaceExisting property on PostedMediaFile instead")]
        Media HandleMedia(int parentNodeId, PostedMediaFile postedFile, User user, bool replaceExisting);
    }
}
