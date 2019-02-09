Dan Garvanski - 100415847
Client-Server Encrypted Communication and User Authentication Software(Beta)
Final Edit - 3:54AM Dec-14-2018


Instructions:

Username: b_mayfield
Password: qb_the_best06


!!BEFORE YOU START, ON THE SERVER SIDE YOU NEED TO PUT THE ADDRESS TO THE USERS FOLDER INTO CODE AT TWO PLACES THAT I HAVE MARKED WITH A COMMENT INSIDE THE PROGRAM OTHERWISE IT IS NOT GOING TO FIND WHERE THE USERS AND PASSWORDS ARE STORED ON THE SERVER!!

1.Start Server and press "Open Sockets".

2.Start Client and press "Connect". If any error occurs during the key exchange the program will close. Just restart it again until it goes past this step.

3.Exchange one or two messages in the chat and use "Decrypt Message" to make sure that the encryption and decryption are working. If not please restart both programs and try again until its translating the encrypted messages successfully.

4.On the client side press the "Log In" and enter the log in details then press "Log In".

5.On the Server side press "Verify user" if the username and password entered by the client are correct, the server will send a message back "Access Granted". If they are incorrect it will send a message back saying eighter the username is wrong or the password.



BEHIND THE SCENES:

SERVER:
The way the software works is, upon clicking "Open Sockets", it creates a new socket, it loads the directory where all the users are located as txt files, it generates random numbers for the prime, base and client-secret number for Deffie-Hellman key exchange, it calculates the servers public key and it waits for incoming connections.

CLIENT:
On the client side you press "Connect" once you are sure that the server sockets are open.(There should be a system message board on the server side saying if the sockets are open.) And it connects to the server with the IP address of 127.0.0.1, port number 3 and it generates its secret key for the DH key exchange

SERVER - CLIENT key exchange:
When an incoming connection comes to the server. It automatically sends the prime number first for the DH key exchange and when the client receives it, the client sends a message back to the server that the prime has been accepted. Upon receiving the message that the prime is accepted it proceeds with sending the base number. The client repeats the steps again and after the server receives a message that the base has been accepted in sends the public key. The client then does the calculations and it sends the Server its public key.
Once they've got all the keys exchanged, they calculate the sharedKeyNumber and assign it as a key for the caesar cipher which is the message encryption method for my software.

Upon successful key exchange, both programs should be in a state of waiting.

SERVER:
The button "Verify User" sends a request to the client to send over login details starting with the username.

CLEINT:
If on the client side the username is = to 0 characters(meaning that the user hasn't entered it yet), a message pop up saying that the server has requested login details.

So on the client side you press the button "Log in" and a new window opens up asking you to input username and password, you input them and press "Log In" and that stores them in variables in the program. Then the server has to click the "Verify User" button again to check the log in details.

SERVER - CLIENT user verification:
Once you know the client has entered login details you click the "Verify user" button again and it sends a request to the client to send the data. The client sends the username and the server checks in a folder names "Users" if a file of format txt, with that name exists, if it does that means the user is registered in the system and the file contains the user password inside. If he doesn't, the server sends a message saying "Wrong user" if the username is in the server database it then sends a message to the client asking for the password. The client then sends the password to the server and the server opens the file whose name matches the username and scans the string inside and checks if it matches the password send by the client.
If it does it sends the beautiful message "Access Granted!" if it does not is sends a message "Password does not match!".


NOTES:
All the messages that are passed between are encrypted. But due to my ambitious plans for the software and the tight deadline I could not organize the code better so the software is highly unstable. It may crash a few times upon the client trying to connect, or it may connect but the DH keys might not calculate the key properly. After connection, I always exchange a few messages back and forth to make sure the decryption works - "Decrypt Message" to decrypt the messages. I hope you will give the program a few attempts to work before you pronounce it dead, as I am sure it will work!


My intentions were to create a third program that would be a File Managment Server and when the client receives the "Acces Granted" message, it would have also received connection details to the File Server and a unique code for verification in the File Server. Unfortunately attempting to stabilize my code took more time than I expected.

Best Regards
Dan Garvanski