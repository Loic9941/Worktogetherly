using MediatR;
using WorkTogetherly.Application.Rules.Common;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Rules.GetAllRules;

public class GetAllRulesHandler(IRuleRepository ruleRepository) : IRequestHandler<GetAllRulesQuery, List<RuleResult>>
{
    public async Task<List<RuleResult>> Handle(GetAllRulesQuery request, CancellationToken cancellationToken)
    {
        var rules = await ruleRepository.GetAllAsync(cancellationToken);
        return rules.Select(r => new RuleResult(r.Id, r.Name)).ToList();
    }
}
