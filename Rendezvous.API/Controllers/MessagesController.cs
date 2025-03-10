using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rendezvous.API.DTOs;
using Rendezvous.API.Entities;
using Rendezvous.API.Extensions;
using Rendezvous.API.Helpers;
using Rendezvous.API.Interfaces;

namespace Rendezvous.API.Controllers;

[Authorize]
public class MessagesController(IMessageRepository messageRepository,
    IUserRepository userRepository, IMapper mapper) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser(
        [FromQuery] MessageParams messageParams)
    {
        messageParams.Username = User.GetUsername();

        var messages = await messageRepository.GetMessagesForUserAsync(messageParams);

        Response.AddPaginationHeader(messages);

        return Ok(messages);
    }

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
    {
        var currentUsername = User.GetUsername();
        return Ok(await messageRepository.GetMessageThreadAsync(currentUsername, username));
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var username = User.GetUsername();

        if (username.ToLower() == createMessageDto.RecipientUsername.ToLower())
        {
            return BadRequest("You cannot message yourself.");
        }

        var sender = await userRepository
            .GetUserByUsernameAsync(username);
        var recipient = await userRepository
            .GetUserByUsernameAsync(createMessageDto.RecipientUsername);

        if (sender == null || recipient == null || sender.UserName == null || recipient.UserName == null)
        {
            return BadRequest("Cannot send message.");
        }

        var message = new Message
        {
            Sender = sender,
            SenderUsername = sender.UserName,
            Recipient = recipient,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };
        await messageRepository.AddMessageAsync(message);

        if (await messageRepository.SaveAllAsync())
        {
            return Ok(mapper.Map<MessageDto>(message));
        }

        return BadRequest("Failed to save message.");
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var username = User.GetUsername();
        var message = await messageRepository.GetMessageAsync(id);

        if (message == null)
        {
            return BadRequest("Cannot delete this message.");
        }

        if (message.SenderUsername != username && message.RecipientUsername != username)
        {
            return Forbid();
        }

        if (message.SenderUsername == username)
        {
            message.SenderDeleted = true;
        }

        if (message.RecipientUsername == username)
        {
            message.RecipientDeleted = true;
        }

        if (message is { SenderDeleted: true, RecipientDeleted: true })
        {
            messageRepository.DeleteMessage(message);
        }

        if (await messageRepository.SaveAllAsync())
        {
            return Ok();
        }

        return BadRequest("Problem with deleting the message.");
    }
}
