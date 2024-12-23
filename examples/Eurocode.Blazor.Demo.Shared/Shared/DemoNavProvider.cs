// ------------------------------------------------------------------------
// MIT License - Copyright (c) Microsoft Corporation. All rights reserved.
// ------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Routing;
using Icons = Microsoft.FluentUI.AspNetCore.Components.Icons;

namespace Eurocode.Blazor.Demo.Shared;

public class DemoNavProvider
{
    internal const string EditFormOffIcon = "<svg style=\"width: 12px; fill: var(--neutral-foreground-rest);\" focusable=\"false\" viewBox=\"0 0 16 16\" aria-hidden=\"true\"><title>This component is not yet compatible with the EditForm.</title><!--!--><path d=\"M5.8 6.5 1.14 1.85a.5.5 0 1 1 .7-.7l13 13a.5.5 0 0 1-.7.7L9.5 10.21l-3.14 3.13c-.37.38-.84.64-1.35.78l-3.39.86a.5.5 0 0 1-.6-.6l.86-3.39c.14-.51.4-.98.78-1.35L5.79 6.5Zm3 3L6.5 7.2l-3.14 3.14c-.24.25-.42.56-.5.9l-.67 2.57 2.57-.66c.34-.1.65-.27.9-.51L8.79 9.5Zm3.24-3.25-1.83 1.84.7.7 3.33-3.32a2.62 2.62 0 0 0-3.71-3.7L7.2 5.08l.7.7 1.84-1.83 2.3 2.29Zm-.8-3.78a1.62 1.62 0 1 1 2.29 2.3l-.78.77-2.3-2.29.79-.78Z\"></path></svg>";

    internal const string Gap = "5px";

    public IReadOnlyList<NavItem> NavMenuItems { get; init; }

    public IReadOnlyList<NavItem> FlattenedMenuItems { get; init; }

    public DemoNavProvider()
    {
        NavMenuItems =
        [
            new NavLink(
                href: "/",
                match: NavLinkMatch.All,
                icon: new Icons.Regular.Size20.Home(),
                title: "Home"
            ),

            new NavGroup(
                icon: new Icons.Regular.Size20.PersonRunning(),
                title: "Getting Started",
                expanded: !true,
                gap: Gap,
                children:
                [
                    new NavLink(
                        href: "/WhatsNew",
                        icon: new Icons.Regular.Size20.Info(),
                        title: "What's new"
                    ),
                ]
            ),



            new NavGroup(
                icon: new Icons.Regular.Size20.PuzzleCubePiece(),
                title: "Components",
                expanded: false,
                gap: Gap,
                children:
                [
                    new NavGroup(
                        title: "EC0 Grondslagen",
                        expanded: !true,
                        gap: Gap,
                        icon: new Icons.Regular.Size20.NumberCircle0(),
                        children:
                        [
                            new NavLink(
                                href: "/grondslagen",
                                icon: new Icons.Regular.Size20.Badge(),
                                title: "Grondslagen"
                            ),
                            new NavLink(
                                href: "/button",
                                icon: new Icons.Regular.Size20.Button(),
                                title: "Button"
                            ),
                        ]
                    ),
                    new NavGroup(
                        title: "EC1 Belastingen",
                        expanded: !true,
                        gap: Gap,
                        icon: new Icons.Regular.Size20.NumberCircle1(),
                        children:
                        [
                            new NavLink(
                                href: "/belastingen/gevallen",
                                icon: new Icons.Regular.Size20.ControlButton(),
                                title: "Belasting gevallen"
                            ),
                            new NavLink(
                                href: "/belastingen/combinaties",
                                icon: new Icons.Regular.Size20.ChevronCircleDown(),
                                title: "Belasting combinaties"
                            ),
                        ]
                    ),
                     new NavGroup(
                        title: "EC2 Beton",
                        expanded: !true,
                        gap: Gap,
                        icon: new Icons.Regular.Size20.NumberCircle2(),
                        children:
                        [
                            new NavLink(
                                href: "/beton/materialen",
                                icon: new Icons.Regular.Size20.Diamond(),
                                title: "Materialen"
                            ),
                            new NavLink(
                                href: "/beton/duurzaamheid",
                                icon: new Icons.Regular.Size20.WeatherBlowingSnow(),
                                title: "Duurzaamheid"
                            ),
                             new NavLink(
                                href: "/beton/buiging",
                                icon: new Icons.Regular.Size20.ArrowTurnLeftDown(),
                                title: "Buiging"
                            ),
                        ]
                    ),
                    new NavGroup(
                        title: "EC3 Staal",
                        expanded: !true,
                        gap: Gap,
                        icon: new Icons.Regular.Size20.NumberCircle3(),
                        children:[]
                    ),
                    new NavGroup(
                        title: "EC4 Staal/Beton",
                        expanded: !true,
                        gap: Gap,
                        icon: new Icons.Regular.Size20.NumberCircle4(),
                        children:[]
                    ),
                     new NavGroup(
                        title: "EC5 Hout",
                        expanded: !true,
                        gap: Gap,
                        icon: new Icons.Regular.Size20.NumberCircle5(),
                        children:[]
                    ),
                     new NavGroup(
                        title: "EC6 Metselwerk",
                        expanded: !true,
                        gap: Gap,
                        icon: new Icons.Regular.Size20.NumberCircle6(),
                        children:[]
                    ),
                     new NavGroup(
                        title: "EC7 Geotechniek",
                        expanded: !true,
                        gap: Gap,
                        icon: new Icons.Regular.Size20.NumberCircle7(),
                        children:[]
                    ),

                ]),

            new NavGroup(
                icon: new Icons.Regular.Size20.Beaker(),
                title: "Incubation lab",
                expanded: false,
                gap: "10px",
                children:
                [
                    new NavLink(
                        href: "/Lab/Overview",
                        icon: new Icons.Regular.Size20.Beaker(),
                        title: "Overview"
                    ),

                    new NavLink(
                        href: "/Lab/MarkdownSection",
                        icon: new Icons.Regular.Size20.ArrowSortDown(),
                        title: "MarkdownSection"
                    ),

                    new NavLink(
                        href: "/Lab/TableOfContents",
                        icon: new Icons.Regular.Size20.DocumentTextLink(),
                        title: "TableOfContents"
                    ),
                    new NavLink(
                        href: "/issue-tester",
                        icon: new Icons.Regular.Size20.WrenchScrewdriver(),
                        title: "Issue Tester"
                    ),
                ]
            )
        ];

        FlattenedMenuItems = GetFlattenedMenuItems(NavMenuItems)
            .ToList()
            .AsReadOnly();
    }

    private static IEnumerable<NavItem> GetFlattenedMenuItems(IEnumerable<NavItem> items)
    {
        foreach (var item in items)
        {
            yield return item;

            if (item is not NavGroup group || !group.Children.Any())
            {
                continue;
            }

            foreach (var flattenedMenuItem in GetFlattenedMenuItems(group.Children))
            {
                yield return flattenedMenuItem;
            }
        }
    }
}
