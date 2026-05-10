using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Infrastructure.Persistence;

public class DatabaseSeeder(AppDbContext context, UserManager<User> userManager)
{
    public async Task SeedAsync()
    {
        if (await context.Users.AnyAsync()) return;

        var users      = await SeedUsersAsync();
        var workspaces = await SeedWorkspacesAsync(users);
        SeedWorkspaceMaterialsAndRules(workspaces);
        var slotMap    = await SeedSlotsAsync(workspaces);
        await SeedBookingsAsync(slotMap, users);
    }

    private async Task<List<User>> SeedUsersAsync()
    {
        var definitions = new[]
        {
            ("Alice",   "Dupont",    "alice.dupont@example.com"),
            ("Bob",     "Martin",    "bob.martin@example.com"),
            ("Céline",  "Lambert",   "celine.lambert@example.com"),
            ("David",   "Renard",    "david.renard@example.com"),
            ("Emma",    "Lecomte",   "emma.lecomte@example.com"),
            ("François","Dubois",    "francois.dubois@example.com"),
            ("Gaëlle",  "Marchand",  "gaelle.marchand@example.com"),
        };

        var result = new List<User>();
        foreach (var (first, last, email) in definitions)
        {
            var user = User.Create(first, last, email);
            var ir = await userManager.CreateAsync(user, "Seed1234");
            if (!ir.Succeeded)
                throw new InvalidOperationException(
                    $"Seed: failed to create user {email}: {string.Join(", ", ir.Errors.Select(e => e.Description))}");
            result.Add(user);
        }
        return result;
    }

    private async Task<List<Workspace>> SeedWorkspacesAsync(List<User> users)
    {
        var alice    = users[0];
        var bob      = users[1];
        var emma     = users[4];
        var francois = users[5];
        var gaelle   = users[6];

        var workspaces = new List<Workspace>
        {
            // ── Braine-l'Alleud ──────────────────────────────────────────────────────
            Workspace.Create(alice.Id,    "Le Bureau Vert",       "Espace de travail calme avec vue sur jardin",             "Rue de la Station 12, Braine-l'Alleud",          50.6851, 4.3835,  6,  true),
            Workspace.Create(alice.Id,    "Studio Calme",         "Studio insonorisé idéal pour les appels vidéo",           "Avenue Reine Astrid 5, Braine-l'Alleud",         50.6878, 4.3792,  4,  true),
            Workspace.Create(alice.Id,    "La Salle Lumineuse",   "Grande salle lumineuse pour équipes et ateliers",         "Chaussée de Waterloo 101, Braine-l'Alleud",      50.6832, 4.3861,  10, true),
            Workspace.Create(emma.Id,     "Le Loft du Village",   "Loft industriel rénové, idéal pour workshops créatifs",   "Rue Sainte-Anne 8, Braine-l'Alleud",             50.6844, 4.3810,  8,  true),
            Workspace.Create(francois.Id, "L'Espace Léon",        "Bureau privé dans maison de maître, jardin disponible",   "Rue Léon Castilhon 3, Braine-l'Alleud",          50.6860, 4.3770,  3,  true),

            // ── Bruxelles & proche périphérie ────────────────────────────────────────
            Workspace.Create(bob.Id,      "Cowork Central",       "Grand open space au cœur de Bruxelles",                   "Rue du Midi 44, Bruxelles",                      50.8463, 4.3521,  8,  true),
            Workspace.Create(bob.Id,      "L'Atelier Partagé",    "Espace créatif avec salle de réunion modulable",          "Place de la Bourse 3, Bruxelles",                50.8487, 4.3498,  5,  true),
            Workspace.Create(gaelle.Id,   "Hub Ixelles",          "Coworking moderne dans le quartier européen",             "Rue du Trône 60, Ixelles",                       50.8363, 4.3702,  7,  true),
            Workspace.Create(gaelle.Id,   "Mezzanine Uccle",      "Espace calme au-dessus d'une librairie, vue sur parc",    "Chaussée de Waterloo 655, Uccle",                50.7992, 4.3601,  4,  true),
            Workspace.Create(francois.Id, "Le Garage Créatif",    "Ancien garage converti en studio polyvalent",             "Rue Vanderkindere 210, Forest",                  50.8115, 4.3468,  6,  true),
        };

        context.Workspaces.AddRange(workspaces);
        await context.SaveChangesAsync();
        return workspaces;
    }

    private void SeedWorkspaceMaterialsAndRules(List<Workspace> ws)
    {
        // Materials: 1=Écran 2=Caméra web 3=Micro 4=Tableau blanc 5=Projecteur 6=Imprimante 7=Casque audio
        context.WorkspaceMaterials.AddRange(
            // WS 0 — Le Bureau Vert
            WorkspaceMaterial.Create(ws[0].Id, 1, 2),
            WorkspaceMaterial.Create(ws[0].Id, 2, 1),
            WorkspaceMaterial.Create(ws[0].Id, 3, 2),

            // WS 1 — Studio Calme
            WorkspaceMaterial.Create(ws[1].Id, 4, 1),
            WorkspaceMaterial.Create(ws[1].Id, 7, 2),

            // WS 2 — La Salle Lumineuse
            WorkspaceMaterial.Create(ws[2].Id, 1, 4),
            WorkspaceMaterial.Create(ws[2].Id, 5, 2),
            WorkspaceMaterial.Create(ws[2].Id, 2, 2),
            WorkspaceMaterial.Create(ws[2].Id, 6, 1),

            // WS 3 — Le Loft du Village
            WorkspaceMaterial.Create(ws[3].Id, 4, 2),
            WorkspaceMaterial.Create(ws[3].Id, 3, 2),
            WorkspaceMaterial.Create(ws[3].Id, 5, 1),

            // WS 4 — L'Espace Léon
            WorkspaceMaterial.Create(ws[4].Id, 1, 1),
            WorkspaceMaterial.Create(ws[4].Id, 7, 1),

            // WS 5 — Cowork Central
            WorkspaceMaterial.Create(ws[5].Id, 1, 3),
            WorkspaceMaterial.Create(ws[5].Id, 5, 1),
            WorkspaceMaterial.Create(ws[5].Id, 6, 1),

            // WS 6 — L'Atelier Partagé
            WorkspaceMaterial.Create(ws[6].Id, 2, 2),
            WorkspaceMaterial.Create(ws[6].Id, 3, 1),
            WorkspaceMaterial.Create(ws[6].Id, 4, 2),

            // WS 7 — Hub Ixelles
            WorkspaceMaterial.Create(ws[7].Id, 1, 3),
            WorkspaceMaterial.Create(ws[7].Id, 2, 2),
            WorkspaceMaterial.Create(ws[7].Id, 5, 1),
            WorkspaceMaterial.Create(ws[7].Id, 6, 1),

            // WS 8 — Mezzanine Uccle
            WorkspaceMaterial.Create(ws[8].Id, 7, 2),
            WorkspaceMaterial.Create(ws[8].Id, 4, 1),

            // WS 9 — Le Garage Créatif
            WorkspaceMaterial.Create(ws[9].Id, 1, 2),
            WorkspaceMaterial.Create(ws[9].Id, 3, 2),
            WorkspaceMaterial.Create(ws[9].Id, 4, 1),
            WorkspaceMaterial.Create(ws[9].Id, 2, 1)
        );

        // Rules: 1=Non-fumeur 2=Animaux acceptés 3=Silence requis 4=Appels interdits 5=Nourriture autorisée
        context.WorkspaceRules.AddRange(
            // WS 0 — Le Bureau Vert
            WorkspaceRule.Create(ws[0].Id, 1),
            WorkspaceRule.Create(ws[0].Id, 3),

            // WS 1 — Studio Calme
            WorkspaceRule.Create(ws[1].Id, 1),
            WorkspaceRule.Create(ws[1].Id, 3),
            WorkspaceRule.Create(ws[1].Id, 4),

            // WS 2 — La Salle Lumineuse
            WorkspaceRule.Create(ws[2].Id, 1),
            WorkspaceRule.Create(ws[2].Id, 3),

            // WS 3 — Le Loft du Village
            WorkspaceRule.Create(ws[3].Id, 5),
            WorkspaceRule.Create(ws[3].Id, 2),

            // WS 4 — L'Espace Léon
            WorkspaceRule.Create(ws[4].Id, 1),
            WorkspaceRule.Create(ws[4].Id, 3),
            WorkspaceRule.Create(ws[4].Id, 4),

            // WS 5 — Cowork Central
            WorkspaceRule.Create(ws[5].Id, 1),
            WorkspaceRule.Create(ws[5].Id, 5),

            // WS 6 — L'Atelier Partagé
            WorkspaceRule.Create(ws[6].Id, 2),
            WorkspaceRule.Create(ws[6].Id, 5),

            // WS 7 — Hub Ixelles
            WorkspaceRule.Create(ws[7].Id, 1),
            WorkspaceRule.Create(ws[7].Id, 5),

            // WS 8 — Mezzanine Uccle
            WorkspaceRule.Create(ws[8].Id, 1),
            WorkspaceRule.Create(ws[8].Id, 3),
            WorkspaceRule.Create(ws[8].Id, 4),

            // WS 9 — Le Garage Créatif
            WorkspaceRule.Create(ws[9].Id, 2),
            WorkspaceRule.Create(ws[9].Id, 5)
        );
    }

    private async Task<Dictionary<(int wsIdx, int day, string band), Slot>> SeedSlotsAsync(List<Workspace> workspaces)
    {
        var today    = DateTime.UtcNow.Date;
        var slotMap  = new Dictionary<(int wsIdx, int day, string band), Slot>();
        var allSlots = new List<Slot>();

        for (int wsIdx = 0; wsIdx < workspaces.Count; wsIdx++)
        {
            var ws = workspaces[wsIdx];
            for (int day = 0; day < 7; day++)
            {
                var d = today.AddDays(day);

                var morning = Slot.Create(ws.Id, d.AddHours(8), d.AddHours(12), ws.Capacity);
                if (morning.IsError) throw new InvalidOperationException(morning.FirstError.Description);
                slotMap[(wsIdx, day, "morning")] = morning.Value;
                allSlots.Add(morning.Value);

                var afternoon = Slot.Create(ws.Id, d.AddHours(13), d.AddHours(17), ws.Capacity);
                if (afternoon.IsError) throw new InvalidOperationException(afternoon.FirstError.Description);
                slotMap[(wsIdx, day, "afternoon")] = afternoon.Value;
                allSlots.Add(afternoon.Value);

                // Evening slots for some workspaces on some days
                bool hasEvening = wsIdx is 0 or 2 or 5 or 7;
                bool isEveningDay = day is 0 or 2 or 4;
                if (hasEvening && isEveningDay)
                {
                    var evening = Slot.Create(ws.Id, d.AddHours(18), d.AddHours(21), ws.Capacity);
                    if (evening.IsError) throw new InvalidOperationException(evening.FirstError.Description);
                    slotMap[(wsIdx, day, "evening")] = evening.Value;
                    allSlots.Add(evening.Value);
                }
            }
        }

        context.Slots.AddRange(allSlots);
        await context.SaveChangesAsync();
        return slotMap;
    }

    private async Task SeedBookingsAsync(
        Dictionary<(int wsIdx, int day, string band), Slot> slotMap,
        List<User> users)
    {
        var alice    = users[0];
        var bob      = users[1];
        var celine   = users[2];
        var david    = users[3];
        var emma     = users[4];
        var francois = users[5];
        var gaelle   = users[6];

        var bookingDefs = new[]
        {
            // Braine-l'Alleud workspaces
            (slotMap[(0, 1, "morning")],    celine.Id,   new TimeOnly(8, 30)),
            (slotMap[(0, 2, "afternoon")],  david.Id,    new TimeOnly(13, 15)),
            (slotMap[(1, 3, "morning")],    david.Id,    new TimeOnly(8, 45)),
            (slotMap[(2, 1, "morning")],    francois.Id, new TimeOnly(8, 0)),
            (slotMap[(2, 2, "afternoon")],  gaelle.Id,   new TimeOnly(13, 0)),
            (slotMap[(3, 0, "morning")],    celine.Id,   new TimeOnly(9, 0)),
            (slotMap[(3, 1, "afternoon")],  david.Id,    new TimeOnly(14, 0)),
            (slotMap[(4, 2, "morning")],    emma.Id,     new TimeOnly(8, 30)),

            // Bruxelles workspaces
            (slotMap[(5, 1, "morning")],    celine.Id,   new TimeOnly(8, 0)),
            (slotMap[(5, 2, "morning")],    bob.Id,      new TimeOnly(9, 0)),
            (slotMap[(6, 1, "afternoon")],  alice.Id,    new TimeOnly(14, 0)),
            (slotMap[(6, 3, "morning")],    emma.Id,     new TimeOnly(8, 15)),
            (slotMap[(7, 0, "afternoon")],  david.Id,    new TimeOnly(13, 30)),
            (slotMap[(7, 2, "morning")],    francois.Id, new TimeOnly(9, 0)),
            (slotMap[(8, 1, "morning")],    gaelle.Id,   new TimeOnly(8, 0)),
            (slotMap[(9, 0, "afternoon")],  celine.Id,   new TimeOnly(13, 0)),
            (slotMap[(9, 3, "morning")],    alice.Id,    new TimeOnly(9, 30)),
        };

        foreach (var (slot, userId, arrival) in bookingDefs)
        {
            var result = Booking.Create(slot.Id, userId, arrival, slot.StartDateTime, slot.EndDateTime);
            if (result.IsError) throw new InvalidOperationException(result.FirstError.Description);
            var booking = result.Value;
            booking.PopDomainEvents(); // suppress BookingCreatedEvent side-effects
            context.Bookings.Add(booking);
        }

        await context.SaveChangesAsync();
    }
}
