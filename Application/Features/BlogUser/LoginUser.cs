﻿using System.ComponentModel.DataAnnotations;
using Application.Common;
using Application.Common.Interface;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Application.Features.BlogUser;

public class LoginUser : FeatureController
{
    private readonly IMediator _mediator;

    public LoginUser(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<SignInResult>> Login(LoginCommand command)
    {
        return await _mediator.Send(command);
    }
}

public class LoginCommand : IRequest<SignInResult>
{   
    [Required]
    public string Login { get; set; }
    [Required]
    [MinLength(PasswordConstraints.MinLength)]
    [StringLength(PasswordConstraints.MaxLength)]
    [RegularExpression(PasswordConstraints.PasswordRegex)]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    public bool RememberMe { get; set; } = false;
}

internal sealed class LoginAccountCommandHandler : IRequestHandler<LoginCommand, SignInResult>
{
    private readonly ICustomSignInService _customSignIn;
    
    public LoginAccountCommandHandler(ICustomSignInService customSignIn)
    {
        _customSignIn = customSignIn;
    }
    
    public async Task<SignInResult> Handle(LoginCommand payLoad, CancellationToken cancellationToken)
    {
        return await _customSignIn.LoginWithEmailOrUsername(payLoad);
    }
    
    public class LoginUserEvent : DomainEvent
    {
        public LoginUserEvent(Entities.BlogUser account)
        {
            Account = account;
        }

        public Entities.BlogUser Account { get; }
    }
}