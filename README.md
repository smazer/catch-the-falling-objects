Describe any important assumptions that you have made in your code.
==================
- I assumed exposing several gameplay variables as SerializedField was useful to allow design tweaks and quicker iteration on game feel.
    - This could also be accomplished with a game config scriptable object or central script
- Assumed Singleton use was acceptable for a short assignment such as this rather than using a service locator or DI framework
- Assumed SerializedField references were preferable to hard-coded reference gathering based on heirarchy/child objects etc
- Assumed falling objects do not need to collide with eachother. Disabled all collisions besides player and objects
- Added special behavior for stars where upon collecting them, up to two spawn and shoot off in a randomized range arc
- Used GenAI artwork to make it a bit more interesting and audio from freesound.org
    - Assumed asset-hunting was not part of the time limit
- Assumed game title should match prompt title, "Catch the Falling Objects"
- Assumed game should be playable with mouse and keyboard

What edge cases have you considered in your code? What edge cases have you yet to handle?
==================
- Game cleans up all still-falling objects on timeout
- Game logic handles any aspect ratio and adjusts bounds based off the camera, uses ScreenToWorldPoint to get this information on startup
    - Does not currently handle resizing mid-game 

What are some things you would like to do if you had more time? Is there anything you would have to change about the design of your current code to do these things? Give a rough outline of how you might implement these ideas.
==================
- I thought it might be interesting to skin it with a hungry squirrel catching nuts/seeds in its mouth and shooting out nuts from peanut shells etc.
- More interesting special behaviors on collection. Might change the current split behavior into a temporary effect on all collectables for a limited time, though this felt like I'd just be adding time to the project and didn't want to spend more than 4 hours.
- Briefly toyed with the idea of using bumpers on the sides of collection area to bounce objects and increase their point value for each bounce, but felt like too much
- Find more sound effects, went with some silly ones for now
- Considered speeding up spawn rate over time, but felt like this would lead to a broken difficulty curve, might be worth exploring time based changes to gameplay