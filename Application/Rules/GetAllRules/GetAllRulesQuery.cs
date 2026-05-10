using MediatR;
using WorkTogetherly.Application.Rules.Common;

namespace WorkTogetherly.Application.Rules.GetAllRules;

public record GetAllRulesQuery : IRequest<List<RuleResult>>;
