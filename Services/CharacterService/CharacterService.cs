using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services.CharacterService
{
    public class CharacterService : ICharacterService
    {
        
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CharacterService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User
            .FindFirstValue(ClaimTypes.NameIdentifier));

        public async Task<ServiceResponse<List<GetCharacterDto>>> AddCharacter(AddCharacterDto newCharacter)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            Characters character = _mapper.Map<Characters>(newCharacter);
            character.User = await _context.Users.FirstOrDefaultAsync(u=> u.Id == GetUserId());

            _context.Characters.Add(character);
            await _context.SaveChangesAsync();
            serviceResponse.Data = await _context.Characters.
            Where(c=>c.User.Id == GetUserId()).
            Select(c=> _mapper.Map<GetCharacterDto>(c)).
            ToListAsync();
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> DeleteCharacter(int id)
        {
            ServiceResponse<List<GetCharacterDto>> response = new ServiceResponse<List<GetCharacterDto>>();
            

            try
            {
                Characters character = await _context.Characters
                    .FirstOrDefaultAsync(c=>c.ID == id && c.User.Id == GetUserId());  
                    if(character != null)
                    {
                        _context.Characters.Remove(character);
                        await _context.SaveChangesAsync();
                        response.Data = _context.Characters
                            .Where(C=> C.User.Id == GetUserId())
                            .Select(c=> _mapper.Map<GetCharacterDto>(c)).ToList();

                    }     
                    else {
                        response.Success = false;
                        response.Message = "Character not found";
                    }       

            }catch(Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters()
        {
            var response = new ServiceResponse<List<GetCharacterDto>>();
            var dbCharacters = await _context.Characters
            .Include(c=> c.Weapon)
            .Include(c=> c.Skills)
            .Where(c=> c.User.Id == GetUserId())
            .ToListAsync();
            response.Data = dbCharacters.Select(c=> _mapper.Map<GetCharacterDto>(c)).ToList();
            return response;

            
        }

        public async Task<ServiceResponse<GetCharacterDto>> GetCharactersByID(int id)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            var dbCharacter = await _context.Characters.
                FirstOrDefaultAsync(c => c.ID == id && c.User.Id == GetUserId());
            serviceResponse.Data = _mapper.Map<GetCharacterDto>(dbCharacter) ;
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto updateCharacter)
        {
            ServiceResponse<GetCharacterDto> response = new ServiceResponse<GetCharacterDto>();

            try
            {
                var character = await _context.Characters
                    .Include(c=> c.Weapon)
                    .Include(c=> c.Skills)
                    .Include(c=> c.User)
                    .FirstOrDefaultAsync(c=>c.ID == updateCharacter.ID);
                    if(character.User.Id == GetUserId())
                    {
                        character.Name = updateCharacter.Name;
                        character.HitPoints = updateCharacter.HitPoints;
                        character.Strength = updateCharacter.Strength;
                        character.Intelligence = updateCharacter.Intelligence;
                        character.Class = updateCharacter.Class;

                        await _context.SaveChangesAsync();

                        response.Data = _mapper.Map<GetCharacterDto>(character);
                    }
                    else {
                        response.Success = false;
                        response.Message = "Character not found";
                    }
                

            }catch(Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<GetCharacterDto>> AddCharacterSkill(AddCharacterSkillDto newCharacterSkill)
        {
            var response = new ServiceResponse<GetCharacterDto>();
            try{
                var character = await _context.Characters
                    .Include(c=> c.Weapon)
                    .Include(c=> c.Skills)
                    .FirstOrDefaultAsync(c => c.ID == newCharacterSkill.CharacterId &&
                    c.User.Id == GetUserId());

                if(character == null){
                    response.Success = false;
                    response.Message = "Character not found";
                    return response;
                }

                var Skill = await _context.Skills.FirstOrDefaultAsync(s => s.Id == newCharacterSkill.SkillId);
                if(Skill == null){
                    response.Success = false;
                    response.Message = "Skill not found.";
                    return response;
                }
                character.Skills.Add(Skill);
                await _context.SaveChangesAsync();
                response.Data = _mapper.Map<GetCharacterDto>(character);

            }catch(Exception ex){
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}