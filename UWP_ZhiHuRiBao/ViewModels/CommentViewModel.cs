﻿using Brook.ZhiHuRiBao.Common;
using Brook.ZhiHuRiBao.Events;
using Brook.ZhiHuRiBao.Models;
using Brook.ZhiHuRiBao.Utils;
using LLQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brook.ZhiHuRiBao.ViewModels
{
    public class CommentViewModel : ViewModelBase
    {
        private CommentType _currCommentType = CommentType.Long;

        private readonly ObservableCollectionExtended<GroupComments> _commentList = new ObservableCollectionExtended<GroupComments>();

        public ObservableCollectionExtended<GroupComments> CommentList { get { return _commentList; } }

        public string LastCommentId
        {
            get { return CommentList.Last().LastOrDefault()?.id.ToString() ?? null; }
        }

        public string Title
        {
            get { return StringUtil.GetString("CommentTitle"); }
        }

        public CommentViewModel()
        {
            LLQNotifier.Default.Register(this);
        }

        public async Task RequestComments(bool isLoadingMore)
        {
            if (!isLoadingMore)
            {
                ResetComments();
                InitCommentInfo();
            }

            if (_currCommentType == CommentType.Long)
            {
                await RequestLongComments(isLoadingMore);
            }
            else if (_currCommentType == CommentType.Short)
            {
                await RequestShortComments();
            }
        }

        private void InitCommentInfo()
        {
            if (CommentList.Count == 0)
            {
                CommentList.Add(new GroupComments() { GroupName = StringUtil.GetCommentGroupName(CommentType.Long, CurrentStoryExtraInfo.long_comments.ToString()) });
                CommentList.Add(new GroupComments() { GroupName = StringUtil.GetCommentGroupName(CommentType.Short, CurrentStoryExtraInfo.short_comments.ToString()) });
            }
        }

        public void ResetComments()
        {
            CommentList.Clear();
            _currCommentType = CommentType.Long;
        }

        private async Task RequestLongComments(bool isLoadingMore)
        {
            var longComment = await DataRequester.RequestLongComment(CurrentStoryId, LastCommentId);
            if (longComment == null)
                return;

            CommentList.First().AddRange(longComment.comments);

            if (longComment == null || longComment.comments.Count < Misc.Page_Count)
            {
                await RequestShortComments();
            }
        }

        private async Task RequestShortComments()
        {
            if (_currCommentType == CommentType.Long)
            {
                _currCommentType = CommentType.Short;
            }
            var shortComment = await DataRequester.RequestShortComment(CurrentStoryId, _currCommentType == CommentType.Long ? null : LastCommentId);
            if (shortComment == null)
                return;

            CommentList.Last().AddRange(shortComment.comments);
        }

        [SubscriberCallback(typeof(StoryExtraEvent))]
        private void Subscriber(StoryExtraEvent param)
        {
            if(CommentList.Count > 1)
            {
                CommentList[0].GroupName = StringUtil.GetCommentGroupName(CommentType.Long, CurrentStoryExtraInfo.long_comments.ToString());
                CommentList[1].GroupName = StringUtil.GetCommentGroupName(CommentType.Short, CurrentStoryExtraInfo.short_comments.ToString());
            }
        }
    }
}
