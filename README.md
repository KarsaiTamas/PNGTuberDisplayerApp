# PNGTuberDisplayerApp
<br>How to use:</br>
<br>Making a scene.</br>
<br>Click on scenes. Than click on Add new Scene on the left to make new scenes.</br>
<br>You can place characters, objects into these scenes, and to toggle a scene just click the scene name.</br>
<br>When you do, click on the Characters button. Than you can create characters here on the left.</br>
<br>These are also toggle buttons. So click on a character name to toggle editing.</br>
<br>If you have a scene open, than an add to scene button appears foreach character, which with you can add them to this scene.</br>
<br>Make sure to click back to scenes and hit save after you did the changes for it.</br>
<br></br>
<br>Making a character:</br>
<br>To change a sprite just click on the image than a file browser pops up to select your sprite.</br>
<br>Only supports PNG as of right now, (and I discovered that if you just drag and drop a webp and just rename it to PNG it still don't work cus... I have no idea why... So if your png doesn't loads than just use "save as" when you save an image or use a program like Krita, Gimp or Photoshop to save your image as PNG.)</br>
<br>IMPORTANT: instead of using whole body images for every action, I use a 4 layer image method.</br>
<br>1 layer for body, 1 layer for eyes, 1 layer for mouth and 1 layer for an outfit</br>
<br>So a working character's sprite example: Body: CharacterBody. Eyes: CharacterEyes1, CharacterEyes2. Mouth: CharacterMouth1, CharacterMouth2. Outfit: CharacterCoolOutfit</br>
<br></br>
<br>SPRITE NAMING:</br>
<br></br>
<br>For 1 frame PNGs you can have normal names if something break here than only use english letters, there shouldn't be a name lenght limit, but just don't have too long file names.</br>
<br>IF you want animations than you have to use a sequence of PNGs with a number sequence behind it ALWAYS STARTING WITH 1(I tried to use gif but godot doesn't support it)</br>
<br>F.e.: For the eyes: CharcaterEyes1, CharacterEyes2, CharacterEyes3... etc</br>
<br>And here select "CharacterEyes1" and under "Blink" Set the Frame count to the number of frames so here that would be 3.</br>
<br></br>
<br>The frame duration is how fast each frame of the animation change. So lower number faster animation.</br>
<br>Make sure to hit save character after you added your animations.</br>
<br>Other animations... SoonTM They don't work yet.</br>
<br>With the top input box you can change your charcter's name, under It this specific outfit's name.</br>
<br></br>
<br>When you add a new character, the new ui has 3 images for now called: Idle aka base image, Talk aka characters mouth, Blink aka eyes.</br>
<br>So for Idle just have your idle pose of your character with no mouth and no eyes.</br>
<br>For Talk you just need your character's mouth an open and close frame.</br>
<br>For Blink you just need your character's eyes an open and close frame.</br>
<br></br>
<br>The top left Green button let's you add in outfits for your character.</br>
<br>Simple outfit is just an overlay for your character so you just need an outfit that fits your character.</br>
<br>Complex outfit makes you setup your character for each frame like you setup your basic character.</br>
<br></br>
<br>To switch between EDITING outfits under the green button you can select which outfit you want to modify. Just MAKE SURE TO SAVE before switching between them.</br>
<br></br>
<br>Modifying the scene:</br>
<br>Hold click on a character to move it around.</br>
<br>Hold Shift+Left mouse button and drag up and down to resize a character</br>
<br>Press F to rotate a character.</br>
<br>Simple click a character to popup a menu, where you can select the outfit for your character and remove it from this scene.</br>
<br>Moving them between layers... SoonTM :D</br>
<br>After you did the changes don't forget to SAVE the scene</br>

<br>This is an app to display multiple png tubers in 1 scene</br>
<br>You can set Input and output audio to be used for your created character.</br>
<br>You can give simple or complex outfits to your characters. For now only the blinking and talking work for animation.</br>
<br>Uses sqlite to save your characters, scenes, and for future animation exectutions.</br>
<br>You should be able to setup any audio for a character to talk with.</br>
<br>Multiplayer support works I believe, BUT needs testing.</br>

<br>Plans:</br> 
<br>Adding in support for database call animations, which makes it so with a twitch bot which can execute database calls, can execute animations.</br>
<br>The update command for this database call will be written here.</br>
<br>Adding in support for hotkey animations, which I only tested if the app was selected, so I'll see if that's possible to do.</br>



<br>note: </br>
<br>I did tested claud in this project. So it made usable code. </br>
<br>The free version is great for bug hunting, so no more spend 5 hours to find a dumb bug.</br>
<br>So It mostly made the audio detection stuff, which I already modified by hand a bit</br>
<br>Networking mostly made by it, but I did check the whole code, and I think I understand how it works now.</br>
<br>I'll see how much it's gonna be used in the end, tho I will check every piece of code it made before implementing it, cus I just don't trust it.</br>
<br>Tho possibly I can mostly replace google with claud, based on my experience with it so far.</br>
