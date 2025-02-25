{ pkgs ? import <nixpkgs> {} }:

pkgs.mkShell {
  buildInputs = [
    pkgs.fish
    pkgs.dotnet-sdk  # or your desired dotnet version
    pkgs.mesa
    pkgs.libglvnd
  ];

  shellHook = ''
    export LD_LIBRARY_PATH=${pkgs.libglvnd}/lib:${pkgs.mesa}/lib:$LD_LIBRARY_PATH
    # If we're not already in fish, set an environment variable and switch to fish.
    if [ -z "$FISH_VERSION" ]; then
      export INSIDE_NIX_SHELL="nix-shell"
      exec fish
    fi
  
  '';
}
