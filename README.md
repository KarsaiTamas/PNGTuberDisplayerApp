# PNGTuberDisplayerApp
<br>How to use:</br>
<br>To make a new scene, click on scenes. Than click on create scene. It will open the newly created scene instantly.</br>
<br>You can place characters, objects into these scenes. If you open the character creator.</br>
<br>To create a new character click: Characters, Create character.</br>
<br></br>
<br>Making a character:</br>
<br>To change a sprite just click on the sprite you want to change that says "click to change than a file browser pops up to select your sprite.</br>
<br>Only supports PNG as of right now, (and I discovered that if you just drag and drop a webp and just rename it to PNG it still don't work cus... I have no idea why... So if your png doesn't loads than just use "save as" when you save an image or use a program like Krita, Gimp or Photoshop to save your image as PNG.)</br>
<br>IMPORTANT: instead of using whole body images for every action, I use a 4 layer image method.</br>
<br>1 layer for body, 1 layer for eyes, 1 layer for mouth and 1 layer for an outfit</br>
<br>So a working character's sprite example: Body: CharacterBody1. Eyes: CharacterEyes1, CharacterEyes2. Mouth: CharacterMouth1, CharacterMouth2. Outfit: CharacterCoolOutfit1</br>
<br>Right now I only setup 1 frame outfits, which after thinking I may gonna allow animated outfits, just I gotta have to change some things to make that work.</br>
<br>SPRITE NAMING:</br>
<br></br>
<br>If something break here than only use english letters, there shouldn't be a name length limit, but just don't have too long file names.</br>
<br>The sprites should always have a number at the end and you should always start with 1, and have a sequence of numbers for the animation.</br>
<br>F.e.: For the eyes: CharcaterEyes1, CharacterEyes2, CharacterEyes3... etc</br>
<br>Here you have to select "CharacterEyes1" when you press the image under "Eyes" which gives you a file select popup for the eyes. Set the Frame count to be the number of frames so here that would be 3.</br>
<br>Frame time is how fast each frame of the animation change. So lower number=faster animation.</br>
<br>Make sure to hit save character after you added your animations.</br>
<br>Other animations... SoonTM They don't work yet... I have an idea for them, but first I want steam joining to work properly.</br>
<br>With the top input box you can change your charcter's name.</br>
<br></br>
<br>When you add a new character, the new ui has 3 images for now called: Body , Eyes, Mouth.</br>
<br>So for Body just have your character's body with no eyes or mouth.</br>
<br>For Eyes you just need your character's eyes an open and close frame.</br>
<br>For Mouth you just need your character's mouth open and close frame tho you can be fancy and give like a hand gesture animation here as well.</br>
<br></br>
<br>You can add in new outfits and animations(soonTM) from the Characters menu.</br>
<br>For now we just have single frame outfits and you can rename them under Outfits, tho make sure to end the outfit's name with '1'. I will leave it like this cus I may add animated outfits.</br>
<br></br>
<br></br>
<br>Modifying the scene:</br>
<br>Hold click on a character to move it around.</br>
<br>Hold Shift+Left mouse button and drag up and down to resize a character</br>
<br>Press F to rotate a character.</br>
<br>Simple click a character to popup a menu, where you can select the outfit for your character and remove it from this scene.</br>
<br>Moving them between layers is done via W or Up arrow, and S or Down arrow.</br>
<br>After you did the changes don't forget to SAVE the scene</br>

<br>This is an app to display multiple png tubers in 1 scene</br>
<br>You can set Input and output audio to be used for your created character.</br>
<br>You should be able to setup any audio for a character to talk with.</br>
<br>Multiplayer is using steam, so you can setup a lobby via hitting host in Network, than you just copy the invite code which is also in Network.</br>
<br>Right now you can join to someone, but I haven't tested successfully sending images between players for the PNGs to work so that's iffy.</br>
<br>The mouth sync haven't been setup yet, since I want to verify if joining works properly now.</br>

<br>Plans:</br> 
<br>Adding in extra animation support which with you can make it interact with let's say twitch chat.</br>
<br>Adding in support for hotkey animations.</br>



<br>note: </br>
<br>I did tested claud in this project. So it made usable code. </br>
<br>The free version is great for bug hunting, so no more spend 5 hours to find a dumb bug.</br>
<br>So It mostly made the audio detection stuff, which I already modified by hand a bit</br>
<br>Networking is mixed right now, some things made by it, some things made by me.</br>
<br>I'll see how much it's gonna be used in the end, tho I will check every piece of code it made before implementing it, cus I just don't trust it.</br>
<br>I mainly gonna use claud to replace google search with it. Google is throwing me AI anyway when seraching for something.</br>
