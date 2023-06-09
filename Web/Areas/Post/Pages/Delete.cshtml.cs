﻿using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Application.Common.Security;
using Application.Entities;
using Application.Features.Post;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace Web.Areas.Post.Pages;

[CustomAuthorize]
public class MediatorDeletePost : PageModel
{
    [BindProperty]
    public DeletePostCommand Input { get; set; } = default!;
    
    public int PostId { get; set; }
    
    public bool IsSuccessful { get; set; }
    
    public virtual Task OnGetAsync(int postId, [StringSyntax(StringSyntaxAttribute.Uri)] string? returnUrl = null) => throw new NotImplementedException();
    
    public virtual Task<IActionResult> OnPostAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? returnUrl = null) => throw new NotImplementedException();
}

internal sealed class MediatorDeletePost<TUser> : MediatorDeletePost where TUser : class
{
    private readonly IMediator _mediator;
    private readonly UserManager<BlogUser> _userManager;
    private readonly SignInManager<BlogUser> _signInManager;
    private readonly NavigationManager _navManager;

    public MediatorDeletePost(IMediator mediator, 
        UserManager<BlogUser> userManager,
        SignInManager<BlogUser> signInManager,
        NavigationManager navManager)
    {
        _mediator = mediator;
        _userManager = userManager;
        _signInManager = signInManager;
        _navManager = navManager;
    }

    public override async Task OnGetAsync(int postId, string? returnUrl = null)
    {
        
        var post = new RetrievePostCommand { Id = postId };
        var response = await _mediator.Send(post);

        if (response.Value == null) { Response.Redirect("/" + _navManager.BaseUri); }
        
        var result = JsonConvert.DeserializeObject<Application.Entities.Post>(response.Value.ToString());
        if (!result.UserId.Equals(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier))) { Response.Redirect("/" + _navManager.BaseUri); }

        Input = new DeletePostCommand
        {
            PostId = postId
        };
    }

    //TODO: ADD ADDITIONAL CHECKS TO THIS...
    public override async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
       if (ModelState.IsValid)
        {
            var result = await _mediator.Send(Input);

            if (result)
            {
                IsSuccessful = true;
            }
        }
       return Page();
    }
}