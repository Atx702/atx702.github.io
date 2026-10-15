namespace LastNight.Core
{
    /// <summary>The 20 achievements from docs/06_Easter_Eggs_Secrets.md. IDs match GameManager.Unlock calls.</summary>
    public static class Achievements
    {
        public readonly struct Info
        {
            public readonly string id, name, description;
            public readonly bool hidden;
            public Info(string id, string name, string description, bool hidden = false)
            {
                this.id = id; this.name = name; this.description = description; this.hidden = hidden;
            }
        }

        public static readonly Info[] All =
        {
            new Info("THERE_YOU_ARE", "There You Are", "Wake in the field."),
            new Info("WRONG_BLUE", "Wrong Blue", "Rebuild the sailboat."),
            new Info("PRESENT", "Present", "Answer roll call."),
            new Info("SCARF_GUY", "Scarf Guy", "Catch the scarf."),
            new Info("THIRD_MUG", "Third Mug", "Hear the news."),
            new Info("FOUR_MINUTES", "Four Minutes", "Reach room 7."),
            new Info("312_AM", "3:12 AM", "Look down.", hidden: true),
            new Info("LITTLE_LANTERN", "Little Lantern", "Sing to Wren.", hidden: true),
            new Info("STAY", "Stay", "Choose to stay.", hidden: true),
            new Info("WAKE", "Wake", "Choose to wake.", hidden: true),
            new Info("MARENS_WORDS", "Maren's Words", "See the secret ending.", hidden: true),
            new Info("SOMETHING_DOESNT_BELONG", "Something Doesn't Belong", "Find all 5 leaked objects."),
            new Info("THE_NIGHTSTAND", "The Nightstand", "Find all 12 of Wren's drawings."),
            new Info("GOOD_BOY", "Good Boy", "Find Biscuit.", hidden: true),
            new Info("0310", "03:10", "Read the coffee receipt.", hidden: true),
            new Info("EYES_OPEN", "Eyes Open", "Survive the kitchen without closing your eyes."),
            new Info("NEVER_CAUGHT", "Never Caught", "Finish the game without the Pull catching you."),
            new Info("WALKED_PAST", "Walked Past", "Ignore the vending machine."),
            new Info("LUCID", "Lucid", "Finish Lucid Mode."),
            new Info("QUACK", "Quack", "Meet Quack-quack the Seventh.", hidden: true),
        };

        public static Info? Find(string id)
        {
            foreach (var a in All) if (a.id == id) return a;
            return null;
        }
    }
}
