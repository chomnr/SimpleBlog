﻿using System.ComponentModel.DataAnnotations;
using Application.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.BlogUser.Recovery;

public class ResetPassword : FeatureController
{
    private readonly IMediator _mediator;
    
    public ResetPassword(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<IdentityResult>> Login(PasswordResetCommand command)
    {
        return await _mediator.Send(command);
    }
}

public class PasswordResetCommand : IRequest<IdentityResult>
{
    [Required]
    public string UserId { get; set; }
    [Required]
    public string ResetToken { get; set; }
    [Required]
    [DataType(DataType.Password)]
    [Compare("ConfirmPassword", ErrorMessage = "Your passwords do not match.")]
    public string Password { get; set; }
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; }
}

public class PasswordResetCommandHandler : IRequestHandler<PasswordResetCommand, IdentityResult>
{
    private readonly UserManager<Entities.BlogUser> _userManager;

    public PasswordResetCommandHandler(UserManager<Entities.BlogUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IdentityResult> Handle(PasswordResetCommand payLoad, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(payLoad.UserId);
        var error = new CustomError();
        if (payLoad.Password != payLoad.ConfirmPassword)
        {
            return IdentityResult.Failed(error.PasswordDoesNotMatch());
        }
        // ResetPassword.cshtml.cs checks for it.
        return await _userManager.ResetPasswordAsync(user, payLoad.ResetToken, payLoad.Password);
    }
    
    public class PasswordResetEvent : DomainEvent
    {
        public PasswordResetEvent(Entities.BlogUser user)
        {
            User = user;
        }

        public Entities.BlogUser User { get; }
    }
}