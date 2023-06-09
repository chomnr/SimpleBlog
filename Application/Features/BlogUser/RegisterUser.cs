﻿using System.ComponentModel.DataAnnotations;
using Application.Common;
using Application.Common.Interface;
using Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Application.Features.BlogUser;

public class RegisterUser : FeatureController
{
    private readonly IMediator _mediator;
    
    public RegisterUser(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<IdentityResult>> Register(RegisterCommand command)
    {   
        var sender = await _mediator.Send(command);
        return await _mediator.Send(command);
    }
}

public class RegisterCommand : IRequest<IdentityResult>
{
    [Required]
    [StringLength(UserConstraints.RealNameMaxLength)]
    [RegularExpression(UserConstraints.RealNameRegex)]
    public string FirstName { get; set; }
    [Required] 
    [StringLength(UserConstraints.RealNameMaxLength)]
    [RegularExpression(UserConstraints.RealNameRegex)]
    public string LastName { get; set; }
    [Required]
    [MinLength(UsernameConstraints.MinLength)]
    [StringLength(UsernameConstraints.MaxLength)]
    [RegularExpression(UsernameConstraints.AllowedCharactersRegex)]
    public string Username { get; set; }
    [Required]
    [StringLength(UserConstraints.EmailMaxLength)]
    [RegularExpression(UserConstraints.EmailRegex)]
    public string Email { get; set; }
    [Required]
    [MinLength(PasswordConstraints.MinLength)]
    [StringLength(PasswordConstraints.MaxLength)]
    [RegularExpression(PasswordConstraints.PasswordRegex, ErrorMessage = "Requires an 1 upper, 1 special and 1 digit.")]
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}

internal sealed class RegisterAccountCommandHandler : IRequestHandler<RegisterCommand, IdentityResult>
{
    private readonly ICustomIdentityService _customIdentityService;
    private readonly IEmailSenderService _emailSender;
    private readonly IConfiguration _configuration;
    private readonly UserManager<Entities.BlogUser> _userManager;
    private readonly IWebHelperService _webHelperService;
    private readonly DatabaseDbContext _context;

    public RegisterAccountCommandHandler(
        ICustomIdentityService customIdentityService, 
        IEmailSenderService emailSender,
        IConfiguration configuration,
        UserManager<Entities.BlogUser> userManager,
        IWebHelperService webHelperService,
        DatabaseDbContext context)
    {
        _customIdentityService = customIdentityService;
        _emailSender = emailSender;
        _configuration = configuration;
        _userManager = userManager;
        _webHelperService = webHelperService;
        _context = context;
    }

    public async Task<IdentityResult> Handle(RegisterCommand payLoad, CancellationToken cancellationToken)
    {
        var user = new Entities.BlogUser { 
            FirstName = payLoad.FirstName, 
            LastName = payLoad.LastName, 
            UserName = payLoad.Username, 
            Email = payLoad.Email,
        };
        
      //  user.DomainEvents.Add(new RegisterUserEvent(user));
        
        var config = _configuration.GetSection("Authentication").GetSection("Email");

        var result = await _customIdentityService.CustomCreateAsync(payLoad, user);

        if (result.Succeeded)
        {
            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var basePath = _webHelperService.GetBaseUrl();
            var confirmPath = $"/auth/confirm-email" +
                              $"?userId={Uri.EscapeDataString(user.Id)}" +
                              $"&verifyToken={Uri.EscapeDataString(emailToken)}";

            var emailConfirmBody = config["EmailConfirmationBody"]
                .Replace("{url}", basePath + confirmPath)
                .Replace("{token}", emailToken)
                .Replace("{userid}", user.Id)
                .Replace("{firstname}", user.FirstName)
                .Replace("{lastname}", user.LastName);

            var formatHtml = $"<a href={basePath + confirmPath}>Confirm Account</a>";
            await _emailSender.SendEmailAsync( payLoad.Email, config["EmailConfirmationSubject"], emailConfirmBody);
            return result;
        }
        return result;
    }
    public class RegisterUserEvent : DomainEvent
    {
        public RegisterUserEvent(Entities.BlogUser account)
        {
            Account = account;
        }

        public Entities.BlogUser Account { get; }
    }
}