using RLNET;
using System;
using RogueSharpV3Tutorial.Systems;
using RogueSharpV3Tutorial.Behaviors;

namespace RogueSharpV3Tutorial.Core
{
    public class Monster : Actor
    {
        public int? TurnsAlerted { get; set; }

        public void DrawStats( RLConsole statConsole, int position )
        {
            // Start at Y=13 which is below the player stats.
            // Multiply the position by 2 to leave a space between each stat
            int yPosition = 13 + ( position * 2 );
        
            // Begin the line by printing the symbol of the monster in the appropriate color
            statConsole.Print( 1, yPosition, Symbol.ToString(), Color );
        
            // Figure out the width of the health bar by dividing current health by max health
            int width = Convert.ToInt32( ( (double) Health / (double) MaxHealth ) * 16.0 );
            int remainingWidth = 16 - width;
        
            // Set the background colors of the health bar to show how damaged the monster is
            statConsole.SetBackColor( 3, yPosition, width, 1, Swatch.Primary );
            statConsole.SetBackColor( 3 + width, yPosition, remainingWidth, 1, Swatch.PrimaryDarkest );
        
            // Print the monsters name over top of the health bar
            statConsole.Print( 2, yPosition, $": {Name}", Swatch.DbLight );
        }

        public virtual void PerformAction( CommandSystem commandSystem )
        {
        var behavior = new StandardMoveAndAttack();
        behavior.Act( this, commandSystem );
        }
    }

}