using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Dtos.Weapon;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services.WeaponService
{
    public class WeaponService : IWeaponService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAcessor;
        private readonly IMapper _mapper;

        public WeaponService(DataContext context, IHttpContextAccessor httpContextAcessor, IMapper mapper)
        {
            _context = context;
            _httpContextAcessor = httpContextAcessor;
            _mapper = mapper;
        }
            
        
        public async Task<ServiceResponse<GetCharacterDto>> Addweapon(AddWeaponDto newWeapon)
        {
            ServiceResponse<GetCharacterDto> response = new ServiceResponse<GetCharacterDto>();
            try
            {
                Characters character = await _context.Characters
                    .FirstOrDefaultAsync(c => c.ID == newWeapon.CharacterId &&
                    c.User.Id == int.Parse(_httpContextAcessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)));
                    if(character == null)
                    {
                        response.Success = false;
                        response.Message = "Character not found.";
                        return response;
                    }

                    Weapon weapon = new Weapon {
                        Name = newWeapon.Name,
                        Damage = newWeapon.Damage,
                        Character = character
                    };

                    _context.Weapons.Add(weapon);
                    await _context.SaveChangesAsync();
                    response.Data = _mapper.Map<GetCharacterDto>(character);

            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

            }
            return response;
        }
    }
}