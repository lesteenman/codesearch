#!/bin/bash

show_help() {
	echo "Usage: cs regex [-(c|C)] [-w]"
	echo ""
	echo "case insensitive searches will be used if the search string contained only lowercase and numericals."
	echo ""
	echo "    -h: Show this help."
	echo "    -C: Force case sensitive searching."
	echo "    -c: Force case insensitive searching."
	echo "    -w: Exact word match."
	echo "    -d: Debug mode."
}

exclude_directories={}
exclude_files={}
if [ -f "$HOME/.cs_config" ]; then
	# Example config:
	# if [[ $PWD == "$HOME/codesearch"* ]]; then
	# 	exclude_directories=(.git)
	# 	exclude_files=(.gitconfig tags)
	# fi
	source $HOME/.cs_config
fi

if [ -z "$1" ]; then
	show_help
else
	OPTIND=1
	DEBUG=false
	C="s"
	W=""
	# Default to case insensitive search if no caps were used

	while getopts "hcCwdv" opt; do
		case "$opt" in
			h|\?)
				show_help
				exit 0
				;;
			C)
				C="s"
				;;
			c)
				C="i"
				;;
			w)
				W="w"
				;;
			d|v)
				DEBUG=true
				;;
		esac
	done
	shift $(expr $OPTIND - 1 )

	exclude_string=""
	for d in ${exclude_directories[@]}; do
		exclude_string="$exclude_string --exclude-dir=$d"
	done
	for f in ${exclude_files[@]}; do
		exclude_string="$exclude_string --exclude=$f"
	done

	SEARCH="$@"
	if [[ $DEBUG == true ]]; then
		echo "Arguments: -TInr$C$W"
		echo "Exclude files: ${exclude_files[*]}"
		echo "Exclude dirs: ${exclude_directories[*]}"
		echo "Full exclude string: $exclude_string"
		echo "Search term: $SEARCH"
	fi

	out=$(ag --color --group --color-path='36' --color-match='91' \
		"$SEARCH")

	if [ "$out" != "" ]; then
		echo "$out" | less -RX
	else
		echo "Nothing matched '$SEARCH'"
	fi
fi
