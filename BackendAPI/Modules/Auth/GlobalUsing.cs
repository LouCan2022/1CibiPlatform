// library
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using Carter;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.Extensions.Configuration;
global using Microsoft.IdentityModel.Tokens;
global using System.Security.Claims;
global using System.Text;
global using System.Reflection;
global using Konscious.Security.Cryptography;
global using System.Security.Cryptography;
global using Microsoft.AspNetCore.Routing;
global using MediatR;
global using Microsoft.AspNetCore.Builder;
global using Mapster;
global using Microsoft.AspNetCore.Http;
global using BuildingBlocks.Exceptions;
global using FluentValidation;


// path
global using Auth.Data.Entities;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Auth.Data.Context;
global using Auth.Service;
global using Auth.DTO;
global using Auth.Data.Repository;
global using Auth.Services;
global using BuildingBlocks.Behaviors;
global using BuildingBlocks.CQRS;
