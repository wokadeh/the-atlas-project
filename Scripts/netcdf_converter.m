netcdf_file_name = 'download.nc';
base_directory = 'data';
meta_data_file_name = 'data.xml';

variables = get_netcdf_data_variables(netcdf_file_name);
create_base_directory(base_directory);
create_tiffs(netcdf_file_name, base_directory, variables);
create_meta_data(meta_data_file_name, variables, base_directory);

function create_tiffs(netcdf_file_name, base_directory, variables)
    % Those four variables are hardcoded because they are expected to be in
    % every netcdf file we are interested in.
    % They just need to be initially read once.
    longitude = ncread(netcdf_file_name, 'longitude');
    latitude = ncread(netcdf_file_name, 'latitude');
    time = ncread(netcdf_file_name, 'time');
    level = ncread(netcdf_file_name, 'level');
    
    % Create tiffs for all relevant variables
    for i = 1 : length(variables)
        variable = variables(i);
        create_variable_tiffs(netcdf_file_name, base_directory, variable, longitude, latitude, time, level);
    end
end

function create_variable_tiffs(necdf_file_name, base_directory, variable, longitude, latitude, time, level)
    data_variable = ncread(necdf_file_name, variable.id);
    
    % We first want to create the directory for the variable
    variable_file_name = lower(variable.name);
    variable_directory = strcat(base_directory, '/', variable_file_name);
    mkdir(variable_directory);
    
    for t = 1 : length(time)
        % We convert the weird netcdf timestamp to something usable
        timestamp = double(time(t))/ 24 + datenum('1900-01-01 00:00:00');

        % Create the directory for the current time stamp
        time_directory = strcat(variable_directory, '/', datestr(timestamp, 'dd-mm-yyyy_HH-MM-SS'));
        mkdir(time_directory);

        for l = 1 : length(level)
            % Get data values
            values = data_variable(:,:,l,t);

            % Map data values to image
            h = pcolor(longitude, latitude, values');
            set(h, 'EdgeColor', 'none');
            set(gca, 'Unit', 'normalized', 'Position', [0 0 1 1]);
            axis off;
            colormap('gray');
            
            % Write actual tiff file to disk
            file_name = strcat(time_directory, '/', variable_file_name, '_', int2str(level(l)));
            print(file_name, '-dtiff');
        end
    end
end

% Retrieves a list of all actual data variable names in a netcdf file
function variables = get_netcdf_data_variables(netcdf_file_name)
    netcdf_id = netcdf.open(netcdf_file_name, 'NC_NOWRITE');
    
    [~, variable_count, ~, ~] = netcdf.inq(netcdf_id);
    
    variables = [];
    for i = 0 : variable_count - 1
        % We are interested in the number of dimensions the variable takes as an "input"
        [variable_name, ~, dimension_ids, ~] = netcdf.inqVar(netcdf_id, i);
        dimension_count = length(dimension_ids);
       
        variable.id = variable_name;
        variable.name = netcdf.getAtt(netcdf_id, i, 'long_name');
        
        % We assume every variable with 4 dimensions is actual data we care about
        if (dimension_count == 4)
            variables = [variables, variable];
        end
    end
    
    netcdf.close(netcdf_id);
end

function create_meta_data(meta_data_file_name, variables, base_directory)
    document = com.mathworks.xml.XMLUtils.createDocument('variables');
    root = document.getDocumentElement;
    
    % Hardcoded for now might change in the future
    root.setAttribute('bit_depth', '8');
    
    for i = 1 : length(variables)
        % Create element for variable
        variable = variables(i);
        variable_element = document.createElement('variable');
        variable_element.setAttribute('name', variable.name);
        
        % Append element to root
        root.appendChild(variable_element);
    end
    
    % Write xml to disk
    xmlwrite(strcat(base_directory, '/', meta_data_file_name), document);
end

function create_base_directory(base_directory)
    if (exist(base_directory, 'dir')) 
       rmdir(base_directory, 's'); 
    end
    mkdir(base_directory);
end