﻿using Application.Features.BlogUser;
using Application.Features.Post;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Application.Common.Interface;

public interface IPostService
{
    Task<bool> CreateAsync(CreatePostCommand command, string userId);
    
    Task<JsonResult> RetrieveAllAsync(RetrievePostsCommand command, bool partial);
    
    Task<JsonResult> RetrieveSpecificAsync(RetrievePostCommand command);
    
    Task<JsonResult> RetrieveAllFromUserAsync(ViewUserCommand command);
    
    
    Task<JsonResult> RetrieveAllByTag(RetrievePostsByTagCommand command);
    
    Task<bool> DeleteAsync(DeletePostCommand command, string userId, string role);
    
    Task<bool> EditAsync(EditPostCommand command, string userId);
}