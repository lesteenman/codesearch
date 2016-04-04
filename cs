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
	echo "Loaded config"
	source $HOME/.cs_config
fi

if [ -z "$1" ]; then
	show_help
else
	OPTIND=1
	DEFAULT_CAPS=true
	DEBUG=false
	C=""
	W=""
	# Default to case insensitive search if no caps were used

	while getopts "hcCwdv" opt; do
		case "$opt" in
			h|\?)
				show_help
				exit 0
				;;
			C)
				C=""
				DEFAULT_CAPS=false
				;;
			c)
				C="i"
				DEFAULT_CAPS=false
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

	# Default to case insensitive if only lower case was used
	if [[ $DEFAULT_CAPS == true && "$@" == $@{,,} ]]; then
		C="i"
	fi

	SEARCH="$@"
	if [[ $DEBUG == true ]]; then
		echo "Default caps: $DEFAULT_CAPS"
		echo "Arguments: -TInr$C$W"
		echo "Exclude dirs: $exclude_directories"
		echo "Exclude files: $exclude_files"
		echo "Search term: $SEARCH"
	fi

	out=$(grep -TInr$C$W \
		--color=always \
		--exclude=$exclude_files \
		--exclude-dir=$exclude_directories \
		"$SEARCH" \
	)

	if [ "$out" != "" ]; then
		echo "$out" | less -RX
	else
		echo "Nothing matched '$1'"
	fi
fi
