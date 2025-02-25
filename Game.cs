﻿using System;
using RLNET;
using RogueSharp.Random;
using RogueSharpV3Tutorial.Core;
using RogueSharpV3Tutorial.Systems;

namespace RogueSharpV3Tutorial
{
   public static class Game
   {
      // The screen height and width are in number of tiles
      private static readonly int _screenWidth = 100;
      private static readonly int _screenHeight = 70;
      private static RLRootConsole _rootConsole;

      // The map console takes up most of the screen and is where the map will be drawn
      private static readonly int _mapWidth = 80;
      private static readonly int _mapHeight = 48;
      private static RLConsole _mapConsole;

      // Below the map console is the message console which displays attack rolls and other information
      private static readonly int _messageWidth = 80;
      private static readonly int _messageHeight = 11;
      private static RLConsole _messageConsole;

      // The stat console is to the right of the map and display player and monster stats
      private static readonly int _statWidth = 20;
      private static readonly int _statHeight = 70;
      private static RLConsole _statConsole;

      // Above the map is the inventory console which shows the players equipment, abilities, and items
      private static readonly int _inventoryWidth = 80;
      private static readonly int _inventoryHeight = 11;
      private static RLConsole _inventoryConsole;

      private static int _mapLevel = 1;
      private static bool _renderRequired = true;

      public static Player Player { get; set; }
      public static DungeonMap DungeonMap { get; private set; }
      public static MessageLog MessageLog { get; private set; }
      public static CommandSystem CommandSystem { get; private set; }
      public static SchedulingSystem SchedulingSystem { get; private set; }

      // We can use this instance of IRandom throughout our game when generating random number
      public static IRandom Random { get; private set; }

      public static void Main()
      {
         // Establish the seed for the random number generator from the current time
         int seed = (int) DateTime.UtcNow.Ticks;
         Random = new DotNetRandom( seed );

         // This must be the exact name of the bitmap font file we are using or it will error.
         string fontFileName = "terminal8x8.png";

         // The title will appear at the top of the console window along with the seed used to generate the level
         string consoleTitle = $"RougeSharp V3 Tutorial - Level {_mapLevel} - Seed {seed}";

         // Create a new MessageLog and print the random seed used to generate the level
         MessageLog = new MessageLog();
         MessageLog.Add( "The rogue arrives on level 1" );
         MessageLog.Add( $"Level created with seed '{seed}'" );

         // Tell RLNet to use the bitmap font that we specified and that each tile is 8 x 8 pixels
         _rootConsole = new RLRootConsole( fontFileName, _screenWidth, _screenHeight, 8, 8, 1.5f, consoleTitle );

         // Initialize the sub consoles that we will Blit to the root console
         _mapConsole = new RLConsole( _mapWidth, _mapHeight );
         _messageConsole = new RLConsole( _messageWidth, _messageHeight );
         _statConsole = new RLConsole( _statWidth, _statHeight );
         _inventoryConsole = new RLConsole( _inventoryWidth, _inventoryHeight );

         SchedulingSystem = new SchedulingSystem();

         MapGenerator mapGenerator = new MapGenerator( _mapWidth, _mapHeight, 20, 13, 7, _mapLevel );
         DungeonMap = mapGenerator.CreateMap();
         DungeonMap.UpdatePlayerFieldOfView();

         CommandSystem = new CommandSystem();

         // Set up a handler for RLNET's Update event
         _rootConsole.Update += OnRootConsoleUpdate;

         // Set up a handler for RLNET's Render event
         _rootConsole.Render += OnRootConsoleRender;

         // Set background color and text for each console so that we can verify they are in the correct positions
         _inventoryConsole.SetBackColor( 0, 0, _inventoryWidth, _inventoryHeight, Swatch.DbWood );
         _inventoryConsole.Print( 1, 1, "Inventory", Colors.TextHeading );

         // Begin RLNET's game loop
         _rootConsole.Run();
      }

      // Event handler for RLNET's Update event
      private static void OnRootConsoleUpdate( object sender, UpdateEventArgs e )
      {
         bool didPlayerAct = false;
         RLKeyPress keyPress = _rootConsole.Keyboard.GetKeyPress();

         if ( CommandSystem.IsPlayerTurn )
         {
            if ( keyPress != null )
            {
               if ( keyPress.Key == RLKey.Up )
               {
                  didPlayerAct = CommandSystem.MovePlayer( Direction.Up );
               }
               else if ( keyPress.Key == RLKey.Down )
               {
                  didPlayerAct = CommandSystem.MovePlayer( Direction.Down );
               }
               else if ( keyPress.Key == RLKey.Left )
               {
                  didPlayerAct = CommandSystem.MovePlayer( Direction.Left );
               }
               else if ( keyPress.Key == RLKey.Right )
               {
                  didPlayerAct = CommandSystem.MovePlayer( Direction.Right );
               }
               else if ( keyPress.Key == RLKey.Escape )
               {
                  _rootConsole.Close();
               }
               else if ( keyPress.Key == RLKey.Period )
               {
                  if ( DungeonMap.CanMoveDownToNextLevel() )
                  {
                     MapGenerator mapGenerator = new MapGenerator( _mapWidth, _mapHeight, 20, 13, 7, ++_mapLevel );
                     DungeonMap = mapGenerator.CreateMap();
                     MessageLog = new MessageLog();
                     CommandSystem = new CommandSystem();
                     _rootConsole.Title = $"RougeSharp RLNet Tutorial - Level {_mapLevel}";
                     didPlayerAct = true;
                  }
               }
            }

            if ( didPlayerAct )
            {
               _renderRequired = true;
               CommandSystem.EndPlayerTurn();
            }
         }
         else
         {
            CommandSystem.ActivateMonsters();
            _renderRequired = true;
         }
      }

      // Event handler for RLNET's Render event
      private static void OnRootConsoleRender( object sender, UpdateEventArgs e )
      {
         // Don't bother redrawing all of the consoles if nothing has changed.
         if ( _renderRequired )
         {
            _mapConsole.Clear();
            _statConsole.Clear();
            _messageConsole.Clear();

            DungeonMap.Draw( _mapConsole, _statConsole );
            Player.Draw( _mapConsole, DungeonMap );
            Player.DrawStats( _statConsole );  
            MessageLog.Draw( _messageConsole );

            // Blit the sub consoles to the root console in the correct locations
            RLConsole.Blit( _mapConsole, 0, 0, _mapWidth, _mapHeight, _rootConsole, 0, _inventoryHeight );
            RLConsole.Blit( _messageConsole, 0, 0, _messageWidth, _messageHeight, _rootConsole, 0, _screenHeight - _messageHeight );
            RLConsole.Blit( _statConsole, 0, 0, _statWidth, _statHeight, _rootConsole, _mapWidth, 0 );
            RLConsole.Blit( _inventoryConsole, 0, 0, _inventoryWidth, _inventoryHeight, _rootConsole, 0, 0 );

            // Tell RLNET to draw the console that we set
            _rootConsole.Draw();

            _renderRequired = false;
         }
      }
   }
}
