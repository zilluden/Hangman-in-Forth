80 CONSTANT BUFFERSIZE
95 CONSTANT UNDERSCORE
26 CONSTANT ALPHABETSIZE

CREATE secretPhrase BUFFERSIZE CHARS ALLOT

CREATE progress BUFFERSIZE CHARS ALLOT
CREATE guessed 26 CHARS ALLOT

: set-to-value ( n c-addr n -- )
	0 DO
		2DUP I + C!
	LOOP
	2DROP
;
0 progress BUFFERSIZE set-to-value
0 guessed ALPHABETSIZE set-to-value

VARIABLE size
VARIABLE tries 6 tries !

\ --------------------------------------------------------------------------------
\ UTILITY WORDS
\ --------------------------------------------------------------------------------
: hang
	KEY DROP
;

: is-upper ( c -- #t | #f )
	64 >
	OVER 91 <
	AND
;

: is-lower ( c -- #t | #f )
	96 >
	OVER 123 <
	AND
;

: is-char ( c -- #t | #f )
	DUP is-upper
	SWAP
	is-lower
	OR
;

: is-num ( c -- #t | #f )
	47 >
	OVER 58 <
	AND
;

: is-space ( c -- #t | #f )
	BL
	=
;

: to-upper ( c -- )
	DUP is-char
	IF UNDERSCORE AND
	THEN
;

: lower-to-upper ( c-addr n -- )
	0 DO
	DUP I + DUP C@ to-upper SWAP C!
	LOOP
	DROP
;

: input-char ( -- a-addr n )
	PAD DUP 1 ACCEPT DROP C@
;

: input-string ( n1 -- c-addr n2 )
	PAD DUP ROT ACCEPT
;

: find-spaces
	size @ 0 DO
		secretPhrase I + C@ BL =
		IF
			1 progress I + C!
		THEN
	LOOP
;


\ --------------------------------------------------------------------------------
\ PHRASE WORDS
\ --------------------------------------------------------------------------------
: print-phrase
	secretPhrase size @ TYPE
;

: print-progress
	CR ." Phrase: "
	size @ 0 DO
		progress I + C@ 1 =
		IF
			secretPhrase I + C@ EMIT
		ELSE
			UNDERSCORE EMIT
		THEN
		SPACE
	LOOP
;

: phrase-guessed? ( -- #t | #f )
	1
	size @ 0 DO
		progress I + C@ AND
	LOOP
;

: valid-phrase? ( c-addr n -- #t | #f )
	-1 ROT ROT
	0 DO
		DUP I + C@ DUP is-char SWAP is-space XOR ROT AND SWAP
	LOOP
	DROP
	DUP

	IF
	ELSE
		CR ." Valid phrases can only contain characters and spaces"
		ROT ROT 2DUP
		CR 34 EMIT TYPE 34 EMIT ."  is invalid"
		ROT
	THEN
;

: input-phrase
	CR ." Please input your secret phrase below."
	BEGIN
	CR ." Input your phrase: " BUFFERSIZE input-string ( c-addr n )
	2DUP valid-phrase?
	0= WHILE
	REPEAT

	2DUP lower-to-upper ( c-addr n )
	DUP size ! ( c-addr n )
	secretPhrase SWAP ( c-addr phrase-addr n )
	CMOVE
	find-spaces
;


\ --------------------------------------------------------------------------------
\ GUESS WORDS
\ --------------------------------------------------------------------------------
: draw-man
	( POLE )
    CR space 95 emit 95 emit 95 emit 95 emit 95 emit
    CR space 124 EMIT space space space 124 EMIT

	( POLE )
    CR space 124 EMIT space space space

	( DRAW HEAD )
	DUP 6 < IF 79 EMIT THEN

	( POLE )
    CR space 124 EMIT space space

	( DRAW TORSO )
	DUP 4 = IF space 124 EMIT THEN
	DUP 3 = IF 47 EMIT 124 EMIT THEN
	DUP 3 < IF 47 EMIT 124 EMIT 92 EMIT THEN

	( POLE )
    CR space 124 EMIT space space

	( DRAW LEGS )
	DUP 2 < IF 47 EMIT THEN space DUP 1 < IF 92 EMIT THEN

	( POLE )
    CR 95 EMIT 124 EMIT 95 EMIT
    DROP
;

: print-guessed
	CR ." Letters used: "
	ALPHABETSIZE 0 DO
		guessed I + C@
		IF
			I 65 + EMIT SPACE
		ELSE
			UNDERSCORE EMIT SPACE
		THEN
	LOOP
;

: in-phrase? ( c -- c #t | #f )
	0 SWAP
	size @ 0 DO
		secretPhrase I + C@ OVER =
		IF
			1 progress I + C! SWAP -1 OR SWAP
		THEN
	LOOP
	SWAP
;

: char-guessed? ( c -- #t | #f )
	DUP
	65 - guessed + DUP C@ 0=
	IF
		1 SWAP C! 0
	ELSE
		DROP -1
	THEN
;

: make-guess
	CR ." Enter your guess: " input-char to-upper
	DUP is-char
	IF
		char-guessed? 0=
		IF
			in-phrase?
			IF
				PAGE
				EMIT ."  is in the phrase"
			ELSE
				PAGE
				EMIT ."  is not in the phrase"
				tries @ 1 - tries !
			THEN
		ELSE
			PAGE
			EMIT ."  has already been guessed"
		THEN
	ELSE
	PAGE
	EMIT ."  is invalid input (Only use characters)"
	THEN
;

: solve-phrase
	CR ." Please input your solution below."
	CR ." Input your solution: "
	size @ input-string
	2DUP lower-to-upper
	2DUP secretPhrase size @ COMPARE
	IF
		PAGE
		." Sorry " 34 EMIT TYPE 34 EMIT ."  is not correct"
		CR ." Game over!"
		HANG
		BYE
	ELSE
		2DROP
		1 progress size @ set-to-value
	THEN
;

\ --------------------------------------------------------------------------------
\ CONTROL WORDS
\ --------------------------------------------------------------------------------
: print-choice-menu
	CR ." (1) Make a letter guess"
	CR ." (2) Make an attempt to solve the phrase"
	CR ." (0) Quit the game"
;

: quit-game
	PAGE
	." We are sad to see you go!"
	CR ." Goodbye!"
	HANG
	BYE
;

: choice-menu
	print-choice-menu
	CR CR ." Input your choice: "
	input-char

	CASE
   		48 OF quit-game ENDOF 		( 0 )
   		49 OF make-guess ENDOF 		( 1 )
		50 OF solve-phrase ENDOF	( 2 )
   		PAGE ." Invalid choice!"
   	ENDCASE

;

: you-win
	PAGE
	." Congratulations, you win!"
	CR ." You guessed the phrase " 34 EMIT print-phrase 34 EMIT ." !"
	print-guessed
	HANG
	BYE
;

: setup
	input-phrase
	PAGE
;

: print-board
	tries @ draw-man
	print-progress
	print-guessed
;

: game-loop
	BEGIN
		tries @ 1 <
		IF
			PAGE
			tries @ draw-man
			CR ." You ran out of guesses"
			CR ." Game over!"
			HANG
			BYE
		THEN
		phrase-guessed? 0=
		WHILE
		print-board
		CR
		choice-menu
	REPEAT
	you-win
;

: hangman
	PAGE
	." Welcome to Hangman in Forth!"
	setup
	game-loop
;


\ --------------------------------------------------------------------------------
\ END WORDS
\ --------------------------------------------------------------------------------

hangman
