using CityMove.API.Dtos;
using CityMove.API.Services;
using CityMove.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CityMove.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtService _jwt;

    public AuthController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager, JwtService jwt)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwt = jwt;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !user.Ativo)
            return Unauthorized(new { erro = "Usuário inválido ou inativo." });

        var ok = await _signInManager.CheckPasswordSignInAsync(user, dto.Senha, false);
        if (!ok.Succeeded)
            return Unauthorized(new { erro = "Credenciais inválidas." });

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expira) = _jwt.GerarToken(user, roles);
        return Ok(new TokenResponseDto(token, expira, user.Nome, user.Email!, roles));
    }

    [HttpPost("registrar")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Registrar([FromBody] RegistroDto dto)
    {
        if (!DbSeederRoles.Contains(dto.Role))
            return BadRequest(new { erro = $"Role inválida. Use: {string.Join(", ", DbSeederRoles)}" });

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true,
            Nome = dto.Nome,
            Role = dto.Role,
            Ativo = true
        };
        var result = await _userManager.CreateAsync(user, dto.Senha);
        if (!result.Succeeded)
            return BadRequest(new { erros = result.Errors.Select(e => e.Description) });

        await _userManager.AddToRoleAsync(user, dto.Role);
        return Ok(new { user.Id, user.Nome, user.Email, user.Role });
    }

    private static readonly string[] DbSeederRoles = { "Admin", "Motorista", "Fiscal", "Passageiro" };
}
